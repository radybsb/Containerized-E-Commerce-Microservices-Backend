namespace OrderService.Api.Services;

public class ShippingClient : IShippingClient
{
    private readonly HttpClient _httpClient;

    public ShippingClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task CreateShipmentAsync(int orderId)
    { // calls the Shipping Service API to create a shipment for the given order ID
        var payload = new { OrderId = orderId };
        await _httpClient.PostAsJsonAsync("api/shipments", payload);
    }
}
