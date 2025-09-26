// Contracts/PromotionDtos.cs
namespace PromotionsService.DTO;

public record PromotionCreateDto(
    string Name,
    string? Description,
    decimal Discount,
    DateTime StartDate,
    DateTime EndDate,
    List<PromotionDetailDto> Details
);

public record PromotionUpdateDto(
    string Name,
    string? Description,
    decimal Discount,
    DateTime StartDate,
    DateTime EndDate,
    List<PromotionDetailDto> Details
);

public record PromotionDetailDto(int ProductCategoryId, string ProductCategoryName);

public record PromotionReadDto(
    int Id,
    string Name,
    string? Description,
    decimal Discount,
    DateTime StartDate,
    DateTime EndDate,
    IEnumerable<PromotionDetailDto> Details
);
