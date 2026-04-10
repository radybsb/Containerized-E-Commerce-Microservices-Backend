namespace ProductService.Api.Events;

public class StockUpdatedEvent
{
    public int ProductId { get; set; }
    public int OldQuantity { get; set; }
    public int NewQuantity { get; set; }
    public DateTime UpdatedAt { get; set; }
}
