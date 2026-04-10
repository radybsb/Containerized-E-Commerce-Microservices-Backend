using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CustomerService.Api.Data;
using CustomerService.Api.Models;
using CustomerService.Api.DTOs;
using CustomerService.Api.Services;
using CustomerService.Api.Events;

namespace CustomerService.Api.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly CustomerDbContext _context;
    private readonly IRabbitMqService _rabbitMqService;

    public CustomersController(CustomerDbContext context, IRabbitMqService rabbitMqService)
    {
        _context = context;
        _rabbitMqService = rabbitMqService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetById(int id)
    {
        var customer = await _context.Customers.FindAsync(id);

        if (customer == null)
            return NotFound();

        var customerDto = new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email
        };

        return Ok(customerDto);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCustomerDto createCustomerDto)
    {
        var customer = new Customer
        {
            Name = createCustomerDto.Name,
            Email = createCustomerDto.Email
        };

        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();

        var customerCreatedEvent = new CustomerCreatedEvent
        {
            CustomerId = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
            CreatedAt = DateTime.UtcNow
        };

        _rabbitMqService.PublishMessage("customer_created", customerCreatedEvent);

        var customerDto = new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email
        };

        return CreatedAtAction(nameof(GetById),
            new { id = customer.Id }, customerDto);
    }
}
