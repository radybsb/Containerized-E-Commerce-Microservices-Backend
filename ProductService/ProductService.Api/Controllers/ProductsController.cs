using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Api.Data;
using ProductService.Api.Models;
using ProductService.Api.DTOs;
using ProductService.Api.Services;
using ProductService.Api.Events;

namespace ProductService.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly ProductsDbContext _context;
    private readonly IRabbitMqService _rabbitMqService;

    public ProductsController(ProductsDbContext context, IRabbitMqService rabbitMqService)
    {
        _context = context;
        _rabbitMqService = rabbitMqService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product is null)
            return NotFound();

        var productDto = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity
        };

        return Ok(productDto);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProductDto createProductDto)
    {
        var product = new Product
        {
            Name = createProductDto.Name,
            Description = createProductDto.Description,
            Price = createProductDto.Price,
            StockQuantity = createProductDto.StockQuantity
        };

        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        var productCreatedEvent = new ProductCreatedEvent
        {
            ProductId = product.Id,
            Name = product.Name,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            CreatedAt = DateTime.UtcNow
        };

        _rabbitMqService.PublishMessage("product_created", productCreatedEvent);

        var productDto = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity
        };

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, productDto);
    }

    [HttpPut("{id}/stock")]
    public async Task<IActionResult> UpdateStock(int id, UpdateStockDto updateStockDto)
    {
        var product = await _context.Products.FindAsync(id);

        if (product is null)
            return NotFound();

        var oldQuantity = product.StockQuantity;
        product.StockQuantity = updateStockDto.Quantity;
        await _context.SaveChangesAsync();

        var stockUpdatedEvent = new StockUpdatedEvent
        {
            ProductId = product.Id,
            OldQuantity = oldQuantity,
            NewQuantity = product.StockQuantity,
            UpdatedAt = DateTime.UtcNow
        };

        _rabbitMqService.PublishMessage("stock_updated", stockUpdatedEvent);

        return NoContent();
    }
}
