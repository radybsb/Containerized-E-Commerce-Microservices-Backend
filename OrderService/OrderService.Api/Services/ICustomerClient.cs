namespace OrderService.Api.Services;

public interface ICustomerClient
{
    Task<bool> CustomerExistsAsync(int customerId);// defines a method that checks if a customer exists by their ID, returning a boolean value indicating the result of the check
}
