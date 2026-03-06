using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShippingService.Api.Data;
using ShippingService.Api.Models;

namespace ShippingService.Api.Controllers;

[ApiController]
[Route("api/shipments")]
public class ShipmentsController : ControllerBase
{
    private readonly ShippingDbContext _context;

    public ShipmentsController(ShippingDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var shipment = await _context.Shipments.FindAsync(id);
        return shipment is null ? NotFound() : Ok(shipment);
    }

    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetByOrderId(int orderId)
    {
        var shipment = await _context.Shipments
            .FirstOrDefaultAsync(s => s.OrderId == orderId);
        return shipment is null ? NotFound() : Ok(shipment);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Shipment shipment)
    {
        shipment.TrackingNumber = Guid.NewGuid().ToString("N")[..12].ToUpper();
        shipment.CreatedAt = DateTime.UtcNow;
        await _context.Shipments.AddAsync(shipment);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = shipment.Id }, shipment);
    }
}
