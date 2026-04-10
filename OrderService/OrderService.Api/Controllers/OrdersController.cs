using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Data;
using OrderService.Api.Models;
using OrderService.Api.Services;
using OrderService.Api.DTOs;
using OrderService.Api.Events;

namespace OrderService.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly OrdersDbContext _context;
    private readonly ICustomerClient _customerClient;
    private readonly IProductClient _productClient;
    private readonly IRabbitMqService _rabbitMqService;

    public OrdersController(OrdersDbContext context, ICustomerClient customerClient, IProductClient productClient, IRabbitMqService rabbitMqService)
    {
        _context = context;
        _customerClient = customerClient;
        _productClient = productClient;
        _rabbitMqService = rabbitMqService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orders = await _context.Orders.ToListAsync();

        var orderDtos = orders.Select(o => new OrderDto
        {
            Id = o.Id,
            CustomerId = o.CustomerId,
            ProductId = o.ProductId,
            Quantity = o.Quantity,
            Total = o.Total
        }).ToList();

        return Ok(orderDtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _context.Orders.FindAsync(id);

        if (order is null)
            return NotFound();

        var orderDto = new OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            ProductId = order.ProductId,
            Quantity = order.Quantity,
            Total = order.Total
        };

        return Ok(orderDto);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderDto createOrderDto)
    {
        var customerExists = await _customerClient.CustomerExistsAsync(createOrderDto.CustomerId);
        if (!customerExists)
            return BadRequest("Customer does not exist.");

        var productExists = await _productClient.ProductExistsAsync(createOrderDto.ProductId);
        if (!productExists)
            return BadRequest("Product does not exist.");

        var order = new Order
        {
            CustomerId = createOrderDto.CustomerId,
            ProductId = createOrderDto.ProductId,
            Quantity = createOrderDto.Quantity,
            Total = createOrderDto.Total
        };

        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        var orderCreatedEvent = new OrderCreatedEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            ProductId = order.ProductId,
            Quantity = order.Quantity,
            Total = order.Total,
            CreatedAt = DateTime.UtcNow
        };

        _rabbitMqService.PublishMessage("order_created", orderCreatedEvent);
        _rabbitMqService.PublishMessage("shipping_order_created", orderCreatedEvent);

        var orderDto = new OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            ProductId = order.ProductId,
            Quantity = order.Quantity,
            Total = order.Total
        };

        return Ok(orderDto);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var order = await _context.Orders.FindAsync(id);

        if (order is null)
            return NotFound();

        var orderDeletedEvent = new OrderDeletedEvent
        {
            OrderId = order.Id,
            ProductId = order.ProductId,
            Quantity = order.Quantity,
            DeletedAt = DateTime.UtcNow
        };

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        _rabbitMqService.PublishMessage("order_deleted", orderDeletedEvent);
        _rabbitMqService.PublishMessage("shipping_order_deleted", orderDeletedEvent);

        return NoContent();
    }
}
