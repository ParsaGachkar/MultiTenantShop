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

public class CartRepositoryTests
{
    private readonly Mock<IMongoCollection<Cart>> _mockCollection;
    private readonly Mock<IMongoClient> _mockClient;
    private readonly Mock<IMongoDatabase> _mockDatabase;
    private readonly MongoDbContext _context;
    private readonly Mock<TenantStore> _mockTenantStore;
    private readonly CartRepository _repository;

    public CartRepositoryTests()
    {
        _mockCollection = new Mock<IMongoCollection<Cart>>();
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
        _mockTenantStore.Setup(s => s.GetCollection<Cart>(It.IsAny<Tenant>(), "Carts"))
            .Returns(_mockCollection.Object);

        _repository = new CartRepository(_mockTenantStore.Object);
    }

    [Fact]
    public async Task CreateAsync_CallsInsertOne()
    {
        var tenant = new Tenant("tenant-1", "Acme", "acme", "basic", "USD", "en", connectionString: null);
        var customer = new Customer("cust-1", "tenant-1", "test@test.com", "Test", "User");
        var cart = new Cart("cart-1", "tenant-1", "cust-1");

        await _repository.CreateAsync(tenant, cart);

        _mockCollection.Verify(c => c.InsertOneAsync(cart, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_CallsReplaceOne()
    {
        var tenant = new Tenant("tenant-1", "Acme", "acme", "basic", "USD", "en", connectionString: null);
        var customer = new Customer("cust-1", "tenant-1", "test@test.com", "Test", "User");
        var cart = new Cart("cart-1", "tenant-1", "cust-1");

        await _repository.UpdateAsync(tenant, cart);

        _mockCollection.Verify(c => c.ReplaceOneAsync(
            It.IsAny<FilterDefinition<Cart>>(),
            cart,
            It.IsAny<ReplaceOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_CallsDeleteOne()
    {
        var tenant = new Tenant("tenant-1", "Acme", "acme", "basic", "USD", "en", connectionString: null);

        await _repository.DeleteAsync(tenant, "cart-1");

        _mockCollection.Verify(c => c.DeleteOneAsync(
            It.IsAny<FilterDefinition<Cart>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}