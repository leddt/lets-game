using System.Linq;
using System.Net;
using System.Text;
using LetsGame.Web.Data;
using LetsGame.Web.Extensions;
using LetsGame.Web.Hubs;
using LetsGame.Web.RecurringTasks;
using LetsGame.Web.Services.EventSystem;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IPNetwork = System.Net.IPNetwork;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var builder = WebApplication.CreateBuilder(args);

#region Configure services

builder.AddServiceDefaults();
builder.AddEventSystem();
builder.AddApplicationDatabase();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDataProtection()
    .PersistKeysToDbContext<ApplicationDbContext>()
    .SetApplicationName("LetsGame");

builder.Services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.AddApplicationAuth();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.KnownIPNetworks.Add(new IPNetwork(IPAddress.Parse("::ffff:169.0.0.0"), 104));
    options.KnownIPNetworks.Add(new IPNetwork(IPAddress.Parse("::ffff:172.0.0.0"), 104));
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                               ForwardedHeaders.XForwardedProto;
});
            
builder.Services.AddRazorPages();

builder.AddApplicationServices();

builder.Services.AddSpaStaticFiles(options =>
{
    options.RootPath = "ClientApp";
});
            
builder.Services.AddSignalR();

#endregion

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    if (args.Contains("run-tasks"))
    {
        var runner = scope.ServiceProvider.GetRequiredService<RecurringTaskRunner>();
        await runner.RunAll();
        return;
    }

    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}

// For debug purposes
// app.Use(async (ctx, next) =>
// {
//     Console.WriteLine("Request IP: {0}", ctx.Connection.RemoteIpAddress);
//     await next();
// });

app.UseForwardedHeaders();

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Lax
});

// For now we'll rely on the hosting platform to redirect to https 
// app.UseHttpsRedirection();
            
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseEventSystem();
app.UseAuthorization();
app.UseWebSockets();
app.UseEndpoints(_ => { });


app.MapDefaultEndpoints();
app.MapGraphQL();
app.MapRazorPages();
app.MapControllers();
app.MapHub<PresenceHub>("/presences");

// Force login before going to SPA
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated != true)
        await context.ChallengeAsync();
    else
        await next();
});
            
app.UseSpaStaticFiles();
app.UseSpa(spa =>
{
    // HACK: Spa Proxy does not support service discovery because they create their own HttpClient which bypasses the defaults
    var frontendServer = app.Configuration["services:frontend:http:0"];
    if (builder.Environment.IsDevelopment() && !string.IsNullOrEmpty(frontendServer))
    {
        spa.UseProxyToSpaDevelopmentServer(frontendServer);
    }
});

app.Run();