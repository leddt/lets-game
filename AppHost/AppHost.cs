var builder = DistributedApplication.CreateBuilder(args);

var pg = builder.AddPostgres("db").WithDataVolume("db_data");

var frontend = builder.AddNpmApp("frontend", "../LetsGame.Frontend", "dev");

builder.AddProject<Projects.LetsGame_Web>("web")
    .WithUrl("http://localhost:5000")
    .WithReference(pg)
    .WaitFor(pg)
    .WaitFor(frontend);

builder.Build().Run();