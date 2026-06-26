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

public class ProductRepositoryTests
{
    private readonly Mock<IMongoCollection<Product>> _mockCollection;
    private readonly Mock<IMongoClient> _mockClient;
    private readonly Mock<IMongoDatabase> _mockDatabase;
    private readonly MongoDbContext _context;
    private readonly Mock<TenantStore> _mockTenantStore;
    private readonly ProductRepository _repository;

    public ProductRepositoryTests()
    {
        _mockCollection = new Mock<IMongoCollection<Product>>();
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
        _mockTenantStore.Setup(s => s.GetCollection<Product>(It.IsAny<Tenant>(), "Products"))
            .Returns(_mockCollection.Object);

        _repository = new ProductRepository(_mockTenantStore.Object);
    }

    [Fact]
    public async Task CreateAsync_CallsInsertOne()
    {
        var tenant = new Tenant("tenant-1", "Acme", "acme", "basic", "USD", "en", connectionString: null);
        var product = new Product("prod-1", "tenant-1", "cat-1", "Test", "Desc", "SKU1", new Money(10, "USD"));

        await _repository.CreateAsync(tenant, product);

        _mockCollection.Verify(c => c.InsertOneAsync(product, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_CallsReplaceOne()
    {
        var tenant = new Tenant("tenant-1", "Acme", "acme", "basic", "USD", "en", connectionString: null);
        var product = new Product("prod-1", "tenant-1", "cat-1", "Test", "Desc", "SKU1", new Money(10, "USD"));

        await _repository.UpdateAsync(tenant, product);

        _mockCollection.Verify(c => c.ReplaceOneAsync(
            It.IsAny<FilterDefinition<Product>>(),
            product,
            It.IsAny<ReplaceOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_CallsDeleteOne()
    {
        var tenant = new Tenant("tenant-1", "Acme", "acme", "basic", "USD", "en", connectionString: null);

        await _repository.DeleteAsync(tenant, "prod-1");

        _mockCollection.Verify(c => c.DeleteOneAsync(
            It.IsAny<FilterDefinition<Product>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}