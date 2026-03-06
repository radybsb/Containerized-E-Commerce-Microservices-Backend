using System.Net;

namespace OrderService.Api.Services;

public class CustomerClient : ICustomerClient
{
    private readonly HttpClient _httpClient;

    public CustomerClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> CustomerExistsAsync(int customerId)
    {
        var response = await _httpClient.GetAsync(
            $"api/customers/{customerId}"); // calls the Customer Service API to check if a customer exists by their ID

        return response.StatusCode == HttpStatusCode.OK; // returns true if the response status code is 200 OK, indicating that the customer exists; otherwise, it returns false
    }
}
