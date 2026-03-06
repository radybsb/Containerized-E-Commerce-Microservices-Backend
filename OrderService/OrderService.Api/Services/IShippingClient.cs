namespace OrderService.Api.Services;

public interface IShippingClient
{
    Task CreateShipmentAsync(int orderId);
}
