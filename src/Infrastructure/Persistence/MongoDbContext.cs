using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MultiTenantShop.Core.Entities;

namespace MultiTenantShop.Infrastructure.Persistence;

public class MongoDbContext
{
    private readonly IMongoClient _client;
    private readonly MongoDbSettings _settings;

    public MongoDbContext(IMongoClient client, IOptions<MongoDbSettings> settings)
    {
        _client = client;
        _settings = settings.Value;
    }

    public virtual IMongoDatabase GetSharedDatabase()
    {
        return _client.GetDatabase(_settings.DatabaseName);
    }

    public virtual IMongoDatabase GetTenantDatabase(string connectionString)
    {
        var mongoUrl = new MongoUrl(connectionString);
        return _client.GetDatabase(mongoUrl.DatabaseName ?? _settings.DatabaseName);
    }

    public virtual IMongoCollection<Tenant> Tenants => GetSharedDatabase().GetCollection<Tenant>("Tenants");
}

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "multitenantshop";
}