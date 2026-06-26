using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Moq;
using MultiTenantShop.Core.Entities;
using MultiTenantShop.Infrastructure.Persistence;
using Xunit;

namespace MultiTenantShop.Tests.Repositories;

public class TenantStoreTests
{
    private readonly Mock<IMongoClient> _mockClient;
    private readonly Mock<IMongoDatabase> _mockSharedDatabase;
    private readonly Mock<IMongoDatabase> _mockTenantDatabase;
    private readonly MongoDbContext _context;
    private readonly TenantStore _tenantStore;

    public TenantStoreTests()
    {
        _mockClient = new Mock<IMongoClient>();
        _mockSharedDatabase = new Mock<IMongoDatabase>();
        _mockTenantDatabase = new Mock<IMongoDatabase>();

        var settings = new MongoDbSettings
        {
            ConnectionString = "mongodb://localhost:27017",
            DatabaseName = "multitenantshop"
        };

        _mockClient.Setup(c => c.GetDatabase(settings.DatabaseName, null))
            .Returns(_mockSharedDatabase.Object);
        _mockClient.Setup(c => c.GetDatabase("megastore", null))
            .Returns(_mockTenantDatabase.Object);

        _context = new MongoDbContext(_mockClient.Object, Microsoft.Extensions.Options.Options.Create(settings));
        _tenantStore = new TenantStore(_context);
    }

    [Fact]
    public void GetCollection_SharedDb_ReturnsSharedCollection()
    {
        var tenant = new Tenant(
            "tenant-1", "Acme", "acme", "basic", "USD", "en",
            connectionString: null);

        var mockCollection = new Mock<IMongoCollection<Product>>();
        _mockSharedDatabase.Setup(d => d.GetCollection<Product>("Products", null))
            .Returns(mockCollection.Object);

        var collection = _tenantStore.GetCollection<Product>(tenant, "Products");

        Assert.Same(mockCollection.Object, collection);
        _mockSharedDatabase.Verify(d => d.GetCollection<Product>("Products", null), Times.Once);
    }

    [Fact]
    public void GetCollection_TenantDb_ReturnsTenantCollection()
    {
        var tenant = new Tenant(
            "tenant-2", "Mega Store", "mega", "pro", "USD", "en",
            connectionString: "mongodb://megastore-db:27017/megastore");

        var mockCollection = new Mock<IMongoCollection<Product>>();
        _mockTenantDatabase.Setup(d => d.GetCollection<Product>("Products", null))
            .Returns(mockCollection.Object);

        var collection = _tenantStore.GetCollection<Product>(tenant, "Products");

        Assert.Same(mockCollection.Object, collection);
        _mockTenantDatabase.Verify(d => d.GetCollection<Product>("Products", null), Times.Once);
    }
}