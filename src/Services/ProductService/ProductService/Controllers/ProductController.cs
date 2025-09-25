using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Contracts;
using ProductService.Data;
using ProductService.Domain;

namespace ProductService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly ProductDbContext _db;

    public ProductController(ProductDbContext db) => _db = db;

    [HttpGet("GetListProducts")]
    public async Task<ActionResult<PagedResult<ProductListItemDto>>> GetListProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? categoryId = null,
        [FromQuery] string? name = null
    )
    {
        var query = _db.Products.AsQueryable();
        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId);
        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(p => p.Name.Contains(name));
        var total = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductListItemDto(p.Id, p.Name, p.Price, p.Qty, p.Active, p.SKU))
            .ToListAsync();
        return Ok(new PagedResult<ProductListItemDto>(items, page, pageSize, total));
    }

    [HttpGet("GetProductById/{id:int}")]
    public async Task<ActionResult<ProductDetailDto>> GetProductById(int id)
    {
        var product = await _db
            .Products.Include(p => p.Category)
            .Include(p => p.VariationValues)
            .ThenInclude(vv => vv.VariationValue)
            .ThenInclude(vv => vv.Variation)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
            return NotFound();
        return Ok(ToDetailDto(product));
    }

    [HttpGet("GetProductByName")]
    public async Task<ActionResult<IEnumerable<ProductListItemDto>>> GetProductByName(
        [FromQuery] string name
    )
    {
        var products = await _db
            .Products.Where(p => p.Name.Contains(name))
            .Select(p => new ProductListItemDto(p.Id, p.Name, p.Price, p.Qty, p.Active, p.SKU))
            .ToListAsync();
        return Ok(products);
    }

    [HttpGet("GetProductByCategoryId/{categoryId:int}")]
    public async Task<ActionResult<IEnumerable<ProductListItemDto>>> GetByCategory(int categoryId)
    {
        var products = await _db
            .Products.Where(p => p.CategoryId == categoryId)
            .Select(p => new ProductListItemDto(p.Id, p.Name, p.Price, p.Qty, p.Active, p.SKU))
            .ToListAsync();
        return Ok(products);
    }

    [HttpPost("Save")]
    public async Task<ActionResult<ProductDetailDto>> Save([FromBody] CreateProductRequest request)
    {
        if (await _db.Products.AnyAsync(p => p.SKU == request.SKU))
            return Conflict("SKU already exists");
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            CategoryId = request.CategoryId,
            Price = request.Price,
            Qty = request.Qty,
            ProductImage = request.ProductImage,
            SKU = request.SKU,
            Active = request.Active,
        };
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        await ApplyVariationValues(product, request.VariationValueIds);
        await _db.SaveChangesAsync();
        // Reload with required navigations to avoid null references in DTO mapping
        product = await _db
            .Products.Include(p => p.Category)
            .Include(p => p.VariationValues)
            .ThenInclude(vv => vv.VariationValue)
            .ThenInclude(v => v.Variation)
            .FirstAsync(p => p.Id == product.Id);

        return CreatedAtAction(
            nameof(GetProductById),
            new { id = product.Id },
            ToDetailDto(product)
        );
    }

    [HttpPut("Update")]
    public async Task<ActionResult<ProductDetailDto>> Update(
        [FromBody] UpdateProductRequest request
    )
    {
        var product = await _db
            .Products.Include(p => p.VariationValues)
            .FirstOrDefaultAsync(p => p.Id == request.Id);
        if (product == null)
            return NotFound();
        product.Name = request.Name;
        product.Description = request.Description;
        product.CategoryId = request.CategoryId;
        product.Price = request.Price;
        product.Qty = request.Qty;
        product.ProductImage = request.ProductImage;
        product.SKU = request.SKU;
        product.Active = request.Active;

        // reset variation links
        _db.ProductVariationValues.RemoveRange(product.VariationValues);
        await _db.SaveChangesAsync();
        await ApplyVariationValues(product, request.VariationValueIds);
        await _db.SaveChangesAsync();
        // Reload with includes to return complete detail
        product = await _db
            .Products.Include(p => p.Category)
            .Include(p => p.VariationValues)
            .ThenInclude(vv => vv.VariationValue)
            .ThenInclude(v => v.Variation)
            .FirstAsync(p => p.Id == product.Id);
        return Ok(ToDetailDto(product));
    }

    [HttpPut("InActive")]
    public async Task<ActionResult> InActive([FromQuery] int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null)
            return NotFound();
        product.Active = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("DeleteProduct")]
    public async Task<ActionResult> DeleteProduct([FromQuery] int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null)
            return NotFound();
        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private async Task ApplyVariationValues(
        Product product,
        Dictionary<int, List<int>> variationValueIds
    )
    {
        if (variationValueIds == null)
            return;
        foreach (var kvp in variationValueIds)
        {
            foreach (var valId in kvp.Value.Distinct())
            {
                var exists = await _db.VariationValues.AnyAsync(v =>
                    v.Id == valId && v.VariationId == kvp.Key
                );
                if (!exists)
                    continue; // skip invalid combos silently for now
                _db.ProductVariationValues.Add(
                    new ProductVariationValue { ProductId = product.Id, VariationValueId = valId }
                );
            }
        }
    }

    private static ProductDetailDto ToDetailDto(Product product)
    {
        var variations = product
            .VariationValues.Where(v => v.VariationValue?.Variation != null)
            .GroupBy(v => v.VariationValue!.Variation.VariationName)
            .Select(g => new ProductVariationDto(g.Key, g.Select(x => x.VariationValue!.Value)))
            .ToList();

        var categoryName = product.Category?.Name ?? string.Empty;

        return new ProductDetailDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Qty,
            product.SKU,
            product.Active,
            product.CategoryId,
            categoryName,
            variations
        );
    }
}
