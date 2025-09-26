// Controllers/PromotionController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PromotionsService.Data;
using PromotionsService.DTO;
using PromotionsService.Messaging;
using PromotionsService.Models;

namespace PromotionsService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PromotionController : ControllerBase
{
    private readonly PromotionDbContext _db;
    private readonly IPromotionEventPublisher _publisher;

    public PromotionController(PromotionDbContext db, IPromotionEventPublisher publisher)
    {
        _db = db;
        _publisher = publisher;
    }

    // Customers: current promotions (StartDate <= now <= EndDate)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PromotionReadDto>>> GetCurrentAsync()
    {
        var now = DateTime.UtcNow;

        var promos = await _db
            .Promotions.Include(p => p.Details)
            .Where(p => p.StartDate <= now && p.EndDate >= now)
            .AsNoTracking()
            .ToListAsync();

        var dto = promos.Select(p => new PromotionReadDto(
            p.Id,
            p.Name,
            p.Description,
            p.Discount,
            p.StartDate,
            p.EndDate,
            p.Details.Select(d => new PromotionDetailDto(
                d.ProductCategoryId,
                d.ProductCategoryName
            ))
        ));

        return Ok(dto);
    }

    [HttpGet("{id:int}", Name = "GetPromotionById")]
    public async Task<ActionResult<PromotionReadDto>> GetByIdAsync(int id)
    {
        var p = await _db
            .Promotions.Include(x => x.Details)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
        if (p is null)
            return NotFound();

        var dto = new PromotionReadDto(
            p.Id,
            p.Name,
            p.Description,
            p.Discount,
            p.StartDate,
            p.EndDate,
            p.Details.Select(d => new PromotionDetailDto(
                d.ProductCategoryId,
                d.ProductCategoryName
            ))
        );
        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<PromotionReadDto>> CreateAsync(
        [FromBody] PromotionCreateDto input,
        [FromServices] IPromotionEventPublisher publisher
    )
    {
        var p = new Promotion
        {
            Name = input.Name,
            Description = input.Description,
            Discount = input.Discount,
            StartDate = input.StartDate,
            EndDate = input.EndDate,
            Details = input
                .Details.Select(d => new PromotionDetail
                {
                    ProductCategoryId = d.ProductCategoryId,
                    ProductCategoryName = d.ProductCategoryName,
                })
                .ToList(),
        };

        _db.Promotions.Add(p);
        await _db.SaveChangesAsync();

        await publisher.PublishPromotionStartedAsync(p);

        return CreatedAtRoute("GetPromotionById", new { id = p.Id }, ToDto(p));
    }

    private static PromotionReadDto ToDto(Promotion p) =>
        new(
            p.Id,
            p.Name,
            p.Description,
            p.Discount,
            p.StartDate,
            p.EndDate,
            p.Details.Select(d => new PromotionDetailDto(
                d.ProductCategoryId,
                d.ProductCategoryName
            ))
        );

    // Admin: update
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] PromotionUpdateDto input)
    {
        var p = await _db.Promotions.Include(x => x.Details).FirstOrDefaultAsync(x => x.Id == id);
        if (p is null)
            return NotFound();

        var wasActive = DateTime.UtcNow >= p.StartDate && DateTime.UtcNow <= p.EndDate;

        p.Name = input.Name;
        p.Description = input.Description;
        p.Discount = input.Discount;
        p.StartDate = input.StartDate;
        p.EndDate = input.EndDate;

        // Replace details
        _db.PromotionDetails.RemoveRange(p.Details);
        p.Details = input
            .Details.Select(d => new PromotionDetail
            {
                ProductCategoryId = d.ProductCategoryId,
                ProductCategoryName = d.ProductCategoryName,
            })
            .ToList();

        await _db.SaveChangesAsync();

        var isActive = DateTime.UtcNow >= p.StartDate && DateTime.UtcNow <= p.EndDate;
        if (!wasActive && isActive)
            await _publisher.PublishPromotionStartedAsync(p);
        if (wasActive && !isActive)
            await _publisher.PublishPromotionEndedAsync(p);

        return NoContent();
    }

    // Admin: delete
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var p = await _db.Promotions.FindAsync(id);
        if (p is null)
            return NotFound();

        var wasActive = DateTime.UtcNow >= p.StartDate && DateTime.UtcNow <= p.EndDate;

        _db.Promotions.Remove(p);
        await _db.SaveChangesAsync();

        if (wasActive)
            await _publisher.PublishPromotionEndedAsync(p);

        return NoContent();
    }
}
