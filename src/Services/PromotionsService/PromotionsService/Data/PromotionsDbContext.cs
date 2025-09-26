// Data/PromotionDbContext.cs
using Microsoft.EntityFrameworkCore;
using PromotionsService.Models;

namespace PromotionsService.Data;

public class PromotionDbContext : DbContext
{
    public PromotionDbContext(DbContextOptions<PromotionDbContext> options)
        : base(options) { }

    public DbSet<Promotion> Promotions => Set<Promotion>();
    public DbSet<PromotionDetail> PromotionDetails => Set<PromotionDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Promotion>(b =>
        {
            b.ToTable("Promotion");
            b.HasKey(x => x.Id);

            b.Property(x => x.Name).IsRequired().HasMaxLength(200);
            b.Property(x => x.Description).HasMaxLength(1000);
            b.Property(x => x.Discount).HasColumnType("decimal(5,2)");

            b.HasMany(x => x.Details)
                .WithOne(d => d.Promotion!)
                .HasForeignKey(d => d.PromotionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PromotionDetail>(b =>
        {
            b.ToTable("Promotion_Details");
            b.HasKey(x => x.Id);

            b.Property(x => x.ProductCategoryName).IsRequired().HasMaxLength(200);
        });
    }
}
