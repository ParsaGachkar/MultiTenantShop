using MultiTenantShop.Core.Entities;
using MultiTenantShop.Core.Enums;

namespace MultiTenantShop.Tests.Domain;

public class ShipmentTests
{
    [Fact]
    public void Constructor_ShouldCreatePending()
    {
        var shipment = new Shipment("s1", "tenant-1", "ord-1");
        Assert.Equal(ShipmentStatus.Pending, shipment.Status);
    }

    [Fact]
    public void AssignCarrier_ShouldSet()
    {
        var shipment = new Shipment("s1", "tenant-1", "ord-1");
        shipment.AssignCarrier("Tipax", "TRK-123");
        Assert.Equal("Tipax", shipment.Carrier);
        Assert.Equal("TRK-123", shipment.TrackingCode);
    }

    [Fact]
    public void Ship_ShouldTransition()
    {
        var shipment = new Shipment("s1", "tenant-1", "ord-1");
        shipment.AssignCarrier("Tipax", "TRK-123");
        shipment.Ship();
        Assert.Equal(ShipmentStatus.PickedUp, shipment.Status);
        Assert.NotNull(shipment.ShippedAt);
    }

    [Fact]
    public void Ship_ShouldThrow_WhenNoCarrier()
    {
        var shipment = new Shipment("s1", "tenant-1", "ord-1");
        Assert.Throws<ArgumentException>(() => shipment.AssignCarrier("", "TRK"));
    }

    [Fact]
    public void MarkDelivered_ShouldWork()
    {
        var shipment = new Shipment("s1", "tenant-1", "ord-1");
        shipment.AssignCarrier("Tipax", "TRK-123");
        shipment.Ship();
        shipment.MarkInTransit();
        shipment.MarkDelivered();
        Assert.Equal(ShipmentStatus.Delivered, shipment.Status);
        Assert.NotNull(shipment.DeliveredAt);
    }

    [Fact]
    public void MarkDelivered_ShouldThrow_WhenNotInTransit()
    {
        var shipment = new Shipment("s1", "tenant-1", "ord-1");
        Assert.Throws<InvalidOperationException>(() => shipment.MarkDelivered());
    }

    [Fact]
    public void MarkFailed_ShouldWork()
    {
        var shipment = new Shipment("s1", "tenant-1", "ord-1");
        shipment.AssignCarrier("Tipax", "TRK-123");
        shipment.Ship();
        shipment.MarkInTransit();
        shipment.MarkFailed();
        Assert.Equal(ShipmentStatus.Failed, shipment.Status);
    }
}
