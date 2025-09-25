using Microsoft.EntityFrameworkCore;

namespace ProductService.Data;

public static class SeedExtensions
{
    public static async Task EnsureMigratedAndSeededAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
        await db.Database.MigrateAsync();

        if (!db.Categories.Any())
        {
            var cat = new Domain.Category { Name = "Default Category" };
            db.Categories.Add(cat);
            await db.SaveChangesAsync();
        }
    }
}
