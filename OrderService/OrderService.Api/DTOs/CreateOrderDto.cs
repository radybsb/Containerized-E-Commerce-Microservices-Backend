namespace OrderService.Api.DTOs;

public class CreateOrderDto
{
    public int CustomerId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Total { get; set; }
}
