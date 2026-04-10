namespace ApiGateway.Models;

public class OrderDetailsDto
{
    public int OrderId { get; set; }
    public int Quantity { get; set; }
    public decimal Total { get; set; }
    public CustomerDto? Customer { get; set; }
    public ProductDto? Product { get; set; }
}
