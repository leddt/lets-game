using System;
using LetsGame.Web.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LetsGame.Web.Services.EventSystem;

public static class EventSystemExtensions
{
    private static bool AxiomIsConfigured(ConfigurationManager configuration)
    {
        return configuration.GetSection("Axiom:ApiToken").Exists() &&
               configuration.GetSection("Axiom:DatasetName").Exists() &&
               configuration.GetSection("Axiom:EdgeDomain").Exists();
    }
    
    extension(WebApplicationBuilder builder)
    {
        public WebApplicationBuilder AddEventSystem()
        {
            if (AxiomIsConfigured(builder.Configuration))
            {
                builder.Services.Configure<AxiomOptions>(builder.Configuration.GetSection("Axiom"));
                builder.Services.AddSingleton<AxiomEventQueue>();
                builder.Services.AddHostedService<AxiomBackgroundWorker>();
                builder.Services.AddScoped<IEventSystem, AxiomEventSystem>();
            }
            else
            {
                Console.WriteLine("No Axiom configuration found. Using ConsoleEventSystem.");
                builder.Services.AddScoped<IEventSystem, ConsoleEventSystem>();
            }

            return builder;
        }
    }

    extension(WebApplication app)
    {
        public WebApplication UseEventSystem()
        {
            app.Use(async (context, next) =>
            {
                var environment = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
                var eventSystem = context.RequestServices.GetRequiredService<IEventSystem>();
                var userManager = context.RequestServices.GetRequiredService<UserManager<AppUser>>();
                
                eventSystem.Enrich(x =>
                {
                    x.Environment = environment.EnvironmentName;
                    x.HostName = context.Request.Host.Host;
                    x.RequestPath = context.Request.Path.Value;
                    x.RequestMethod = context.Request.Method;
                    x.RemoteIp = context.Connection.RemoteIpAddress?.ToString();
                    x.UserAgent = context.Request.Headers["User-Agent"];
                    
                    x.UserId = userManager.GetUserId(context.User);
                });

                try
                {
                    await next();
                }
                catch (Exception ex)
                {
                    eventSystem.Enrich(x =>
                    {
                        x.ExceptionTypeName = ex.GetType().FullName;
                        x.ExceptionMessage = ex.Message;
                        x.ExceptionStackTrace = ex.StackTrace;
                    });
                }
                finally
                {
                    eventSystem.Enrich(x =>
                    {
                        x.RecordEndTime();;
                        x.StatusCode = context.Response.StatusCode;
                    });
                    
                    await eventSystem.PostAsync();
                }
            });

            return app;
        }
    }
}