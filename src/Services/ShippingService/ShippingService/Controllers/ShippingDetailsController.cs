using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShippingService.Data;
using ShippingService.DTO;
using ShippingService.Models;

namespace ShippingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShippingDetailsController : ControllerBase
{
    private readonly ShippingDbContext _db;

    private readonly IHttpClientFactory _httpClientFactory;

    public ShippingDetailsController(ShippingDbContext db, IHttpClientFactory httpClientFactory)
    {
        _db = db;
        _httpClientFactory = httpClientFactory;
    }

    // POST /api/ShippingDetails
    [HttpPost]
    public async Task<ActionResult<ShippingDetail>> Create([FromBody] CreateShippingDetailDto dto)
    {
        if (!await _db.Shippers.AnyAsync(s => s.Id == dto.Shipper_Id))
            return BadRequest("Invalid Shipper_Id");

        var entity = new ShippingDetail
        {
            Order_Id = dto.Order_Id,
            Shipper_Id = dto.Shipper_Id,
            Shipping_Status = ShippingStatus.Pending,
            Tracking_Number = dto.Tracking_Number,
        };

        _db.Shipping_Details.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetByOrder), new { orderId = entity.Order_Id }, entity);
    }

    // GET /api/ShippingDetails/{orderId}
    [HttpGet("{orderId:int}")]
    public async Task<ActionResult<ShippingDetail>> GetByOrder(int orderId)
    {
        var detail = await _db
            .Shipping_Details.Include(sd => sd.Shipper)
            .AsNoTracking()
            .FirstOrDefaultAsync(sd => sd.Order_Id == orderId);

        return detail is null ? NotFound() : Ok(detail);
    }

    // PUT /api/ShippingDetails/{id}/status
    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateShippingStatusDto dto)
    {
        var entity = await _db.Shipping_Details.FindAsync(id);
        if (entity is null)
            return NotFound();

        entity.Shipping_Status = dto.Status;
        entity.Tracking_Number = dto.Tracking_Number ?? entity.Tracking_Number;

        await _db.SaveChangesAsync();

        // Notify OrderService
        var client = _httpClientFactory.CreateClient("OrderService");
        var payload = new { status = entity.Shipping_Status.ToString() };
        var resp = await client.PutAsJsonAsync(
            $"/orders/{entity.Order_Id}/shipping-status",
            payload
        );
        resp.EnsureSuccessStatusCode();
        return NoContent();
    }
}
