using Microsoft.AspNetCore.Mvc;
using ApiGateway.Models;
using System.Text.Json;

namespace ApiGateway.Controllers;

[ApiController]
[Route("api/aggregated")]
public class AggregationController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AggregationController> _logger;

    public AggregationController(IHttpClientFactory httpClientFactory, ILogger<AggregationController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [HttpGet("order-details/{orderId}")]
    public async Task<ActionResult<OrderDetailsDto>> GetOrderDetails(int orderId)
    {
        var httpClient = _httpClientFactory.CreateClient();

        try
        {
            // Fetch order
            var orderResponse = await httpClient.GetAsync($"http://orderservice:8080/api/orders/{orderId}");
            if (!orderResponse.IsSuccessStatusCode)
                return NotFound("Order not found");

            var orderJson = await orderResponse.Content.ReadAsStringAsync();
            var order = JsonSerializer.Deserialize<OrderDto>(orderJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (order == null)
                return NotFound("Order not found");

            // Fetch customer
            var customerResponse = await httpClient.GetAsync($"http://customerservice:8080/api/customers/{order.CustomerId}");
            CustomerDto? customer = null;
            if (customerResponse.IsSuccessStatusCode)
            {
                var customerJson = await customerResponse.Content.ReadAsStringAsync();
                customer = JsonSerializer.Deserialize<CustomerDto>(customerJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            // Fetch product
            var productResponse = await httpClient.GetAsync($"http://productservice:8080/api/products/{order.ProductId}");
            ProductDto? product = null;
            if (productResponse.IsSuccessStatusCode)
            {
                var productJson = await productResponse.Content.ReadAsStringAsync();
                product = JsonSerializer.Deserialize<ProductDto>(productJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            var orderDetails = new OrderDetailsDto
            {
                OrderId = order.Id,
                Quantity = order.Quantity,
                Total = order.Total,
                Customer = customer,
                Product = product
            };

            return Ok(orderDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching order details");
            return StatusCode(500, "Internal server error");
        }
    }
}
