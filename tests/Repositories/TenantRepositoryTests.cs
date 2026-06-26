using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Moq;
using MultiTenantShop.Core.Entities;
using MultiTenantShop.Infrastructure.Persistence;
using MultiTenantShop.Infrastructure.Repositories;
using Xunit;

namespace MultiTenantShop.Tests.Repositories;

public class TenantRepositoryTests
{
    private readonly Mock<IMongoCollection<Tenant>> _mockCollection;
    private readonly Mock<IMongoClient> _mockClient;
    private readonly Mock<IMongoDatabase> _mockDatabase;
    private readonly MongoDbContext _context;
    private readonly TenantRepository _repository;

    public TenantRepositoryTests()
    {
        _mockCollection = new Mock<IMongoCollection<Tenant>>();
        _mockClient = new Mock<IMongoClient>();
        _mockDatabase = new Mock<IMongoDatabase>();

        var settings = new MongoDbSettings
        {
            ConnectionString = "mongodb://localhost:27017",
            DatabaseName = "multitenantshop"
        };

        _mockClient.Setup(c => c.GetDatabase(settings.DatabaseName, null))
            .Returns(_mockDatabase.Object);
        _mockDatabase.Setup(d => d.GetCollection<Tenant>("Tenants", null))
            .Returns(_mockCollection.Object);

        _context = new MongoDbContext(_mockClient.Object, Microsoft.Extensions.Options.Options.Create(settings));
        _repository = new TenantRepository(_context);
    }

    [Fact]
    public async Task CreateAsync_CallsInsertOne()
    {
        var tenant = new Tenant("tenant-1", "Acme", "acme", "basic", "USD", "en");

        await _repository.CreateAsync(tenant);

        _mockCollection.Verify(c => c.InsertOneAsync(tenant, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_CallsReplaceOne()
    {
        var tenant = new Tenant("tenant-1", "Acme", "acme", "basic", "USD", "en");

        await _repository.UpdateAsync(tenant);

        _mockCollection.Verify(c => c.ReplaceOneAsync(
            It.IsAny<FilterDefinition<Tenant>>(),
            tenant,
            It.IsAny<ReplaceOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_CallsDeleteOne()
    {
        await _repository.DeleteAsync("tenant-1");

        _mockCollection.Verify(c => c.DeleteOneAsync(
            It.IsAny<FilterDefinition<Tenant>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}