namespace ShippingService.Api.DTOs;

public class ShipmentDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
