namespace ProductService.Contracts;

public record ProductListItemDto(
    int Id,
    string Name,
    decimal Price,
    int Qty,
    bool Active,
    string SKU
);

public record ProductDetailDto(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    int Qty,
    string SKU,
    bool Active,
    int CategoryId,
    string CategoryName,
    IEnumerable<ProductVariationDto> Variations
);

public record ProductVariationDto(string VariationName, IEnumerable<string> Values);

public record CreateProductRequest(
    string Name,
    string? Description,
    int CategoryId,
    decimal Price,
    int Qty,
    string? ProductImage,
    string SKU,
    bool Active,
    Dictionary<int, List<int>> VariationValueIds
);

public record UpdateProductRequest(
    int Id,
    string Name,
    string? Description,
    int CategoryId,
    decimal Price,
    int Qty,
    string? ProductImage,
    string SKU,
    bool Active,
    Dictionary<int, List<int>> VariationValueIds
);

public record PagedResult<T>(IEnumerable<T> Items, int Page, int PageSize, int Total);
