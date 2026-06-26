using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Moq;
using MultiTenantShop.Core.Entities;
using MultiTenantShop.Core.Enums;
using MultiTenantShop.Core.ValueObjects;
using MultiTenantShop.Infrastructure.Persistence;
using MultiTenantShop.Infrastructure.Repositories;
using Xunit;

namespace MultiTenantShop.Tests.Repositories;

public class OrderRepositoryTests
{
    private readonly Mock<IMongoCollection<Order>> _mockCollection;
    private readonly Mock<IMongoClient> _mockClient;
    private readonly Mock<IMongoDatabase> _mockDatabase;
    private readonly MongoDbContext _context;
    private readonly Mock<TenantStore> _mockTenantStore;
    private readonly OrderRepository _repository;

    public OrderRepositoryTests()
    {
        _mockCollection = new Mock<IMongoCollection<Order>>();
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
        _mockTenantStore.Setup(s => s.GetCollection<Order>(It.IsAny<Tenant>(), "Orders"))
            .Returns(_mockCollection.Object);

        _repository = new OrderRepository(_mockTenantStore.Object);
    }

    [Fact]
    public async Task CreateAsync_CallsInsertOne()
    {
        var tenant = new Tenant("tenant-1", "Acme", "acmecme", "basic", "USD", "en", connectionString: null);
        var order = new Order("order-1", "tenant-1", "cust-1", "ORD-001", "USD");

        await _repository.CreateAsync(tenant, order);

        _mockCollection.Verify(c => c.InsertOneAsync(order, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_CallsReplaceOne()
    {
        var tenant = new Tenant("tenant-1", "Acme", "acme", "basic", "USD", "en", connectionString: null);
        var order = new Order("order-1", "tenant-1", "cust-1", "ORD-001", "USD");

        await _repository.UpdateAsync(tenant, order);

        _mockCollection.Verify(c => c.ReplaceOneAsync(
            It.IsAny<FilterDefinition<Order>>(),
            order,
            It.IsAny<ReplaceOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}