var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis");
var rabbitmq = builder.AddRabbitMQ("rabbitmq");

var web = builder.AddProject<Projects.MultiTenantShop_Web>("web")
    .WithExternalHttpEndpoints()
    .WithReference(redis)
    .WithReference(rabbitmq)
    .WithEnvironment("OTEL_EXPORTER_OTLP_ENABLED", "true");

builder.Build().Run();
