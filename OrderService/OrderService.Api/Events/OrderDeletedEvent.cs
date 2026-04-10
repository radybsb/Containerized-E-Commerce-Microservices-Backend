namespace OrderService.Api.Events;

public class OrderDeletedEvent
{
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime DeletedAt { get; set; }
}
