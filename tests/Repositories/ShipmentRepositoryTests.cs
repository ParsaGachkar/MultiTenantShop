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

public class ShipmentRepositoryTests
{
    private readonly Mock<IMongoCollection<Shipment>> _mockCollection;
    private readonly Mock<IMongoClient> _mockClient;
    private readonly Mock<IMongoDatabase> _mockDatabase;
    private readonly MongoDbContext _context;
    private readonly Mock<TenantStore> _mockTenantStore;
    private readonly ShipmentRepository _repository;

    public ShipmentRepositoryTests()
    {
        _mockCollection = new Mock<IMongoCollection<Shipment>>();
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
        _mockTenantStore.Setup(s => s.GetCollection<Shipment>(It.IsAny<Tenant>(), "Shipments"))
            .Returns(_mockCollection.Object);

        _repository = new ShipmentRepository(_mockTenantStore.Object);
    }

    [Fact]
    public async Task CreateAsync_CallsInsertOne()
    {
        var tenant = new Tenant("tenant-1", "Acme", "acme", "basic", "USD", "en", connectionString: null);
        var address = new Address("123 St", "City", "State", "12345", "US");
        var shipment = new Shipment("ship-1", "tenant-1", "order-1", address);

        await _repository.CreateAsync(tenant, shipment);

        _mockCollection.Verify(c => c.InsertOneAsync(shipment, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_CallsReplaceOne()
    {
        var tenant = new Tenant("tenant-1", "Acme", "acme", "basic", "USD", "en", connectionString: null);
        var address = new Address("123 St", "City", "State", "12345", "US");
        var shipment = new Shipment("ship-1", "tenant-1", "order-1", address);

        await _repository.UpdateAsync(tenant, shipment);

        _mockCollection.Verify(c => c.ReplaceOneAsync(
            It.IsAny<FilterDefinition<Shipment>>(),
            shipment,
            It.IsAny<ReplaceOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}