using Microsoft.EntityFrameworkCore;
using ShippingService.Models;

namespace ShippingService.Data;

public class ShippingDbContext : DbContext
{
    public ShippingDbContext(DbContextOptions<ShippingDbContext> options)
        : base(options) { }

    public DbSet<Region> Regions => Set<Region>();
    public DbSet<Shipper> Shippers => Set<Shipper>();
    public DbSet<ShipperRegion> Shipper_Regions => Set<ShipperRegion>();
    public DbSet<ShippingDetail> Shipping_Details => Set<ShippingDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Region
        modelBuilder.Entity<Region>().Property(r => r.Name).IsRequired();

        // Shipper
        modelBuilder.Entity<Shipper>().Property(s => s.Name).IsRequired();

        // ShipperRegion (link)
        modelBuilder
            .Entity<ShipperRegion>()
            .HasOne(sr => sr.Region)
            .WithMany(r => r.ShipperRegions)
            .HasForeignKey(sr => sr.Region_Id)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<ShipperRegion>()
            .HasOne(sr => sr.Shipper)
            .WithMany(s => s.Regions)
            .HasForeignKey(sr => sr.Shipper_Id)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<ShipperRegion>()
            .HasIndex(sr => new { sr.Region_Id, sr.Shipper_Id })
            .IsUnique(); // each shipper-region pair once

        // ShippingDetail
        modelBuilder
            .Entity<ShippingDetail>()
            .HasOne(sd => sd.Shipper)
            .WithMany(s => s.Shipments)
            .HasForeignKey(sd => sd.Shipper_Id)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
