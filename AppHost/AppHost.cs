var builder = DistributedApplication.CreateBuilder(args);

var pg = builder
    .AddPostgres("db")
    .WithDataVolume();

var frontendDir = Path.GetFullPath("../LetsGame.Frontend", builder.AppHostDirectory);
var frontend = builder
    .AddNpmApp("frontend", frontendDir, "dev")
    .WithNpmPackageInstallation()
    .WithHttpEndpoint(env: "PORT");

builder.AddProject<Projects.LetsGame_Web>("web")
    .WithReference(pg)
    .WithReference(frontend)
    .WaitFor(pg)
    .WaitFor(frontend);

builder.Build().Run();