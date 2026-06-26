var builder = DistributedApplication.CreateBuilder(args);

var mongo = builder.AddMongoDB("mongodb")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithMongoExpress();

var redis = builder.AddRedis("redis")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithRedisCommander();

var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithManagementPlugin();

var web = builder.AddProject<Projects.MultiTenantShop_Web>("web")
    .WithExternalHttpEndpoints()
    .WithReference(mongo)
    .WithReference(redis)
    .WithReference(rabbitmq)
    .WithEnvironment("OTEL_EXPORTER_OTLP_ENABLED", "true");

builder.Build().Run();