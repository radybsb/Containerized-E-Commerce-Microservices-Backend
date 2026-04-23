using System.Net.Http.Json;
using Frontend.Models;

namespace Frontend.Services
{
  public class ApiService
  {
    private readonly HttpClient _http;
    public ApiService(HttpClient http)
    {
      _http = http;
    }

    // Product methods
    public async Task<List<ProductDto>> GetProductsAsync()
    {
      return await _http.GetFromJsonAsync<List<ProductDto>>("/api/products") ?? new();
    }

    public async Task<ProductDto> CreateProductAsync(ProductDto product)
    {
      var response = await _http.PostAsJsonAsync("/api/products", product);
      response.EnsureSuccessStatusCode();
      return await response.Content.ReadFromJsonAsync<ProductDto>();
    }

    // Customer methods
    public async Task<List<CustomerDto>> GetCustomersAsync()
    {
      return await _http.GetFromJsonAsync<List<CustomerDto>>("/api/customers") ?? new();
    }

    public async Task<CustomerDto> CreateCustomerAsync(CustomerDto customer)
    {
      var response = await _http.PostAsJsonAsync("/api/customers", customer);
      response.EnsureSuccessStatusCode();
      return await response.Content.ReadFromJsonAsync<CustomerDto>();
    }

    // Order methods
    public async Task<List<OrderDto>> GetOrdersAsync()
    {
      return await _http.GetFromJsonAsync<List<OrderDto>>("/api/orders") ?? new();
    }

    public async Task<OrderDto> CreateOrderAsync(OrderDto order)
    {
      var response = await _http.PostAsJsonAsync("/api/orders", order);
      response.EnsureSuccessStatusCode();
      return await response.Content.ReadFromJsonAsync<OrderDto>();
    }
  }
}
