using Microsoft.EntityFrameworkCore;
using ShippingService.Api.Models;

namespace ShippingService.Api.Data;

public class ShippingDbContext : DbContext
{
    public ShippingDbContext(DbContextOptions<ShippingDbContext> options) : base(options) { }

    public DbSet<Shipment> Shipments => Set<Shipment>();
}
