namespace ProductService.Domain;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public decimal Price { get; set; }
    public int Qty { get; set; }
    public string? ProductImage { get; set; }
    public string SKU { get; set; } = string.Empty;
    public bool Active { get; set; } = true;
    public ICollection<ProductVariationValue> VariationValues { get; set; } =
        new List<ProductVariationValue>();
}
