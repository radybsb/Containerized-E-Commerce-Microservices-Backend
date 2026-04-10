using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShippingService.Api.Data;
using ShippingService.Api.Models;
using ShippingService.Api.DTOs;

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

        if (shipment is null)
            return NotFound();

        var shipmentDto = new ShipmentDto
        {
            Id = shipment.Id,
            OrderId = shipment.OrderId,
            Status = shipment.Status,
            TrackingNumber = shipment.TrackingNumber,
            CreatedAt = shipment.CreatedAt
        };

        return Ok(shipmentDto);
    }

    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetByOrderId(int orderId)
    {
        var shipment = await _context.Shipments
            .FirstOrDefaultAsync(s => s.OrderId == orderId);

        if (shipment is null)
            return NotFound();

        var shipmentDto = new ShipmentDto
        {
            Id = shipment.Id,
            OrderId = shipment.OrderId,
            Status = shipment.Status,
            TrackingNumber = shipment.TrackingNumber,
            CreatedAt = shipment.CreatedAt
        };

        return Ok(shipmentDto);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateShipmentDto createShipmentDto)
    {
        var shipment = new Shipment
        {
            OrderId = createShipmentDto.OrderId,
            TrackingNumber = Guid.NewGuid().ToString("N")[..12].ToUpper(),
            CreatedAt = DateTime.UtcNow
        };

        await _context.Shipments.AddAsync(shipment);
        await _context.SaveChangesAsync();

        var shipmentDto = new ShipmentDto
        {
            Id = shipment.Id,
            OrderId = shipment.OrderId,
            Status = shipment.Status,
            TrackingNumber = shipment.TrackingNumber,
            CreatedAt = shipment.CreatedAt
        };

        return CreatedAtAction(nameof(GetById), new { id = shipment.Id }, shipmentDto);
    }
}
