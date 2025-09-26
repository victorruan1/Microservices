// Domain/Promotion.cs
namespace PromotionsService.Models;

public class Promotion
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Discount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ICollection<PromotionDetail> Details { get; set; } = new List<PromotionDetail>();
}

public class PromotionDetail
{
    public int Id { get; set; }

    // FK
    public int PromotionId { get; set; }
    public Promotion? Promotion { get; set; }

    public int ProductCategoryId { get; set; }
    public string ProductCategoryName { get; set; } = string.Empty;
}
