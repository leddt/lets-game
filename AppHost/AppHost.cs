var builder = DistributedApplication.CreateBuilder(args);

var pg = builder
    .AddPostgres("db")
    .WithDataVolume();

var frontend = builder
    .AddNpmApp("frontend", "../LetsGame.Frontend", "dev")
    .WithNpmPackageInstallation()
    .WithHttpEndpoint(env: "PORT");

builder.AddProject<Projects.LetsGame_Web>("web")
    .WithUrl("http://localhost:5000")
    .WithReference(pg)
    .WithReference(frontend)
    .WaitFor(pg)
    .WaitFor(frontend);

builder.Build().Run();