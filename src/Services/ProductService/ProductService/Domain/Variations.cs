namespace ProductService.Domain;

public class CategoryVariation
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public string VariationName { get; set; } = string.Empty;
    public ICollection<VariationValue> Values { get; set; } = new List<VariationValue>();
}

public class VariationValue
{
    public int Id { get; set; }
    public int VariationId { get; set; }
    public CategoryVariation Variation { get; set; } = null!;
    public string Value { get; set; } = string.Empty;
    public ICollection<ProductVariationValue> ProductVariationValues { get; set; } =
        new List<ProductVariationValue>();
}

public class ProductVariationValue
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int VariationValueId { get; set; }
    public VariationValue VariationValue { get; set; } = null!;
}
