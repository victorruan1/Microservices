using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Domain;

namespace ProductService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoryController : ControllerBase
{
    private readonly ProductDbContext _db;

    public CategoryController(ProductDbContext db) => _db = db;

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetAll() =>
        Ok(
            await _db
                .Categories.Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.ParentCategoryId,
                })
                .ToListAsync()
        );

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<object>> Get(int id)
    {
        var c = await _db.Categories.FindAsync(id);
        if (c == null)
            return NotFound();
        return Ok(
            new
            {
                c.Id,
                c.Name,
                c.ParentCategoryId,
            }
        );
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] Category category)
    {
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return CreatedAtAction(
            nameof(Get),
            new { id = category.Id },
            new
            {
                category.Id,
                category.Name,
                category.ParentCategoryId,
            }
        );
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, [FromBody] Category dto)
    {
        var c = await _db.Categories.FindAsync(id);
        if (c == null)
            return NotFound();
        c.Name = dto.Name;
        c.ParentCategoryId = dto.ParentCategoryId;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var c = await _db.Categories.FindAsync(id);
        if (c == null)
            return NotFound();
        _db.Categories.Remove(c);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoryVariationController : ControllerBase
{
    private readonly ProductDbContext _db;

    public CategoryVariationController(ProductDbContext db) => _db = db;

    [AllowAnonymous]
    [HttpGet("ByCategory/{categoryId:int}")]
    public async Task<ActionResult<IEnumerable<object>>> GetByCategory(int categoryId) =>
        Ok(
            await _db
                .CategoryVariations.Where(v => v.CategoryId == categoryId)
                .Select(v => new
                {
                    v.Id,
                    v.CategoryId,
                    v.VariationName,
                })
                .ToListAsync()
        );

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CategoryVariation variation)
    {
        _db.CategoryVariations.Add(variation);
        await _db.SaveChangesAsync();
        return CreatedAtAction(
            nameof(GetByCategory),
            new { categoryId = variation.CategoryId },
            new
            {
                variation.Id,
                variation.CategoryId,
                variation.VariationName,
            }
        );
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductVariationController : ControllerBase
{
    private readonly ProductDbContext _db;

    public ProductVariationController(ProductDbContext db) => _db = db;

    [AllowAnonymous]
    [HttpGet("Values/{variationId:int}")]
    public async Task<ActionResult<IEnumerable<object>>> GetValues(int variationId) =>
        Ok(
            await _db
                .VariationValues.Where(v => v.VariationId == variationId)
                .Select(v => new
                {
                    v.Id,
                    v.VariationId,
                    v.Value,
                })
                .ToListAsync()
        );

    [Authorize(Roles = "Admin")]
    [HttpPost("Values")]
    public async Task<ActionResult> CreateValue([FromBody] VariationValue value)
    {
        _db.VariationValues.Add(value);
        await _db.SaveChangesAsync();
        return CreatedAtAction(
            nameof(GetValues),
            new { variationId = value.VariationId },
            new
            {
                value.Id,
                value.VariationId,
                value.Value,
            }
        );
    }
}
