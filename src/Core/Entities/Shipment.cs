using MultiTenantShop.Core.Enums;
using MultiTenantShop.Core.ValueObjects;

namespace MultiTenantShop.Core.Entities;

public class Shipment
{
    public string ShipmentId { get; private set; }
    public string OrderId { get; private set; }
    public string? Carrier { get; private set; }
    public string? TrackingCode { get; private set; }
    public ShipmentStatus Status { get; private set; }
    public Address? ShippingAddress { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }

    public Shipment(string shipmentId, string orderId, Address? shippingAddress = null)
    {
        ShipmentId = shipmentId;
        OrderId = orderId;
        Status = ShipmentStatus.Pending;
        ShippingAddress = shippingAddress;
    }

    public void AssignCarrier(string carrier, string trackingCode)
    {
        if (string.IsNullOrWhiteSpace(carrier))
            throw new ArgumentException("Carrier is required", nameof(carrier));
        if (string.IsNullOrWhiteSpace(trackingCode))
            throw new ArgumentException("Tracking code is required", nameof(trackingCode));

        Carrier = carrier;
        TrackingCode = trackingCode;
    }

    public void Ship()
    {
        if (Status != ShipmentStatus.Pending)
            throw new InvalidOperationException("Shipment is not in pending state");

        Status = ShipmentStatus.PickedUp;
        ShippedAt = DateTime.UtcNow;
    }

    public void MarkInTransit()
    {
        if (Status != ShipmentStatus.PickedUp)
            throw new InvalidOperationException("Shipment must be picked up first");

        Status = ShipmentStatus.InTransit;
    }

    public void MarkDelivered()
    {
        if (Status != ShipmentStatus.InTransit)
            throw new InvalidOperationException("Shipment must be in transit first");

        Status = ShipmentStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
    }

    public void MarkFailed()
    {
        if (Status is ShipmentStatus.Delivered)
            throw new InvalidOperationException("Cannot fail a delivered shipment");

        Status = ShipmentStatus.Failed;
    }
}
