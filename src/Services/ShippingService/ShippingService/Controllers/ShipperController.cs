using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShippingService.Data;
using ShippingService.DTO;
using ShippingService.Models;

namespace ShippingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShipperController : ControllerBase
{
    private readonly ShippingDbContext _db;

    public ShipperController(ShippingDbContext db) => _db = db;

    // GET /api/Shipper
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Shipper>>> GetAll() =>
        await _db
            .Shippers.Include(s => s.Regions)
            .ThenInclude(sr => sr.Region)
            .AsNoTracking()
            .ToListAsync();

    // GET /api/Shipper/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Shipper>> Get(int id)
    {
        var shipper = await _db
            .Shippers.Include(s => s.Regions)
            .ThenInclude(sr => sr.Region)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        return shipper is null ? NotFound() : Ok(shipper);
    }

    // POST /api/Shipper
    [HttpPost]
    public async Task<ActionResult<Shipper>> Create([FromBody] CreateShipperDto dto)
    {
        var entity = new Shipper
        {
            Name = dto.Name,
            EmailId = dto.EmailId,
            Phone = dto.Phone,
            Contact_Person = dto.Contact_Person,
        };

        _db.Shippers.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, entity);
    }

    // PUT /api/Shipper
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateShipperDto dto)
    {
        var shipper = await _db.Shippers.FindAsync(dto.Id);
        if (shipper is null)
            return NotFound();

        shipper.Name = dto.Name;
        shipper.EmailId = dto.EmailId;
        shipper.Phone = dto.Phone;
        shipper.Contact_Person = dto.Contact_Person;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /api/Shipper/delete-{id}
    [HttpDelete("delete-{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var shipper = await _db.Shippers.FindAsync(id);
        if (shipper is null)
            return NotFound();

        _db.Shippers.Remove(shipper);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // GET /api/Shipper/region/{region}
    [HttpGet("region/{region}")]
    public async Task<ActionResult<IEnumerable<Shipper>>> GetByRegion(string region)
    {
        var data = await _db
            .Shipper_Regions.Include(sr => sr.Shipper)
            .Include(sr => sr.Region)
            .Where(sr => sr.Active && sr.Region != null && sr.Region.Name == region)
            .Select(sr => sr.Shipper!)
            .AsNoTracking()
            .Distinct()
            .ToListAsync();

        return Ok(data);
    }
}
