using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Moq;
using MultiTenantShop.Core.Entities;
using MultiTenantShop.Core.ValueObjects;
using MultiTenantShop.Infrastructure.Persistence;
using MultiTenantShop.Infrastructure.Repositories;
using Xunit;

namespace MultiTenantShop.Tests.Repositories;

public class RefundRepositoryTests
{
    private readonly Mock<IMongoCollection<Refund>> _mockCollection;
    private readonly Mock<IMongoClient> _mockClient;
    private readonly Mock<IMongoDatabase> _mockDatabase;
    private readonly MongoDbContext _context;
    private readonly Mock<TenantStore> _mockTenantStore;
    private readonly RefundRepository _repository;

    public RefundRepositoryTests()
    {
        _mockCollection = new Mock<IMongoCollection<Refund>>();
        _mockClient = new Mock<IMongoClient>();
        _mockDatabase = new Mock<IMongoDatabase>();

        var settings = new MongoDbSettings
        {
            ConnectionString = "mongodb://localhost:27017",
            DatabaseName = "multitenantshop"
        };

        _mockClient.Setup(c => c.GetDatabase(settings.DatabaseName, null))
            .Returns(_mockDatabase.Object);

        _context = new MongoDbContext(_mockClient.Object, Microsoft.Extensions.Options.Options.Create(settings));

        _mockTenantStore = new Mock<TenantStore>(_context) { CallBase = true };
        _mockTenantStore.Setup(s => s.GetCollection<Refund>(It.IsAny<Tenant>(), "Refunds"))
            .Returns(_mockCollection.Object);

        _repository = new RefundRepository(_mockTenantStore.Object);
    }

    [Fact]
    public async Task CreateAsync_CallsInsertOne()
    {
        var tenant = new Tenant("tenant-1", "Acme", "acme", "basic", "USD", "en", connectionString: null);
        var refund = new Refund("ref-1", "tenant-1", "order-1", "pay-1", new Money(50, "USD"), "Customer request", true);

        await _repository.CreateAsync(tenant, refund);

        _mockCollection.Verify(c => c.InsertOneAsync(refund, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_CallsReplaceOne()
    {
        var tenant = new Tenant("tenant-1", "Acme", "acme", "basic", "USD", "en", connectionString: null);
        var refund = new Refund("ref-1", "tenant-1", "order-1", "pay-1", new Money(50, "USD"), "Customer request", true);

        await _repository.UpdateAsync(tenant, refund);

        _mockCollection.Verify(c => c.ReplaceOneAsync(
            It.IsAny<FilterDefinition<Refund>>(),
            refund,
            It.IsAny<ReplaceOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}