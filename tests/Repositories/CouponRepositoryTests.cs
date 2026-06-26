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

public class CouponRepositoryTests
{
    private readonly Mock<IMongoCollection<Coupon>> _mockCollection;
    private readonly Mock<IMongoClient> _mockClient;
    private readonly Mock<IMongoDatabase> _mockDatabase;
    private readonly MongoDbContext _context;
    private readonly Mock<TenantStore> _mockTenantStore;
    private readonly CouponRepository _repository;

    public CouponRepositoryTests()
    {
        _mockCollection = new Mock<IMongoCollection<Coupon>>();
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
        _mockTenantStore.Setup(s => s.GetCollection<Coupon>(It.IsAny<Tenant>(), "Coupons"))
            .Returns(_mockCollection.Object);

        _repository = new CouponRepository(_mockTenantStore.Object);
    }

    [Fact]
    public async Task CreateAsync_CallsInsertOne()
    {
        var tenant = new Tenant("tenant-1", "Acme", "acme", "basic", "USD", "en", connectionString: null);
        var coupon = new Coupon("coupon-1", "tenant-1", "SAVE10", DiscountType.Percentage, 10, DateTime.UtcNow.AddDays(30), new Money(100, "USD"));

        await _repository.CreateAsync(tenant, coupon);

        _mockCollection.Verify(c => c.InsertOneAsync(coupon, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_CallsReplaceOne()
    {
        var tenant = new Tenant("tenant-1", "Acme", "acme", "basic", "USD", "en", connectionString: null);
        var coupon = new Coupon("coupon-1", "tenant-1", "SAVE10", DiscountType.Percentage, 10, DateTime.UtcNow.AddDays(30), new Money(100, "USD"));

        await _repository.UpdateAsync(tenant, coupon);

        _mockCollection.Verify(c => c.ReplaceOneAsync(
            It.IsAny<FilterDefinition<Coupon>>(),
            coupon,
            It.IsAny<ReplaceOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_CallsDeleteOne()
    {
        var tenant = new Tenant("tenant-1", "Acme", "acme", "basic", "USD", "en", connectionString: null);

        await _repository.DeleteAsync(tenant, "coupon-1");

        _mockCollection.Verify(c => c.DeleteOneAsync(
            It.IsAny<FilterDefinition<Coupon>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}