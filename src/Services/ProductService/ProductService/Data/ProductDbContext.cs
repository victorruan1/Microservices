using Microsoft.EntityFrameworkCore;
using ProductService.Domain;

namespace ProductService.Data;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options)
        : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CategoryVariation> CategoryVariations => Set<CategoryVariation>();
    public DbSet<VariationValue> VariationValues => Set<VariationValue>();
    public DbSet<ProductVariationValue> ProductVariationValues => Set<ProductVariationValue>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).IsRequired().HasMaxLength(200);
            e.HasOne(c => c.Parent)
                .WithMany(c => c.Children)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Product>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Name).IsRequired().HasMaxLength(250);
            e.Property(p => p.SKU).IsRequired().HasMaxLength(100);
            e.Property(p => p.Price).HasPrecision(18, 2);
            e.HasIndex(p => p.SKU).IsUnique();
            e.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CategoryVariation>(e =>
        {
            e.HasKey(v => v.Id);
            e.Property(v => v.VariationName).IsRequired().HasMaxLength(150);
            e.HasOne(v => v.Category)
                .WithMany(c => c.Variations)
                .HasForeignKey(v => v.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<VariationValue>(e =>
        {
            e.HasKey(vv => vv.Id);
            e.Property(vv => vv.Value).IsRequired().HasMaxLength(150);
            e.HasOne(vv => vv.Variation)
                .WithMany(v => v.Values)
                .HasForeignKey(vv => vv.VariationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductVariationValue>(e =>
        {
            e.HasKey(pvv => new { pvv.ProductId, pvv.VariationValueId });
            e.HasOne(pvv => pvv.Product)
                .WithMany(p => p.VariationValues)
                .HasForeignKey(pvv => pvv.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(pvv => pvv.VariationValue)
                .WithMany(vv => vv.ProductVariationValues)
                .HasForeignKey(pvv => pvv.VariationValueId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
