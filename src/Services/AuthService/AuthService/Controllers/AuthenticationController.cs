using System.Security.Claims;
using AuthService.Contracts;
using AuthService.Data;
using AuthService.Domain;
using AuthService.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly AuthDbContext _db;
    private readonly JwtTokenService _jwt;

    public AuthenticationController(AuthDbContext db, JwtTokenService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    [HttpPost("login")]
    public async Task<ActionResult<object>> Login(LoginDto dto)
    {
        var user = await _db
            .Users.Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u =>
                u.Username == dto.UsernameOrEmail || u.Email == dto.UsernameOrEmail
            );
        if (user is null)
            return Unauthorized();
        if (!PasswordHasher.Verify(dto.Password, user.PasswordHash, user.Salt))
            return Unauthorized();
        var roles = user.UserRoles.Select(ur => ur.Role!.Name);
        var token = _jwt.CreateToken(user, roles);
        return Ok(new { token });
    }

    [HttpPost("register-admin")]
    public async Task<ActionResult> RegisterAdmin(RegisterDto dto)
    {
        return await RegisterWithRole(dto, roleName: "Admin");
    }

    [HttpPost("customer-register")]
    public async Task<ActionResult> RegisterCustomer(RegisterDto dto)
    {
        return await RegisterWithRole(dto, roleName: "Customer");
    }

    private async Task<ActionResult> RegisterWithRole(RegisterDto dto, string roleName)
    {
        if (await _db.Users.AnyAsync(u => u.Username == dto.Username || u.Email == dto.Email))
            return Conflict("Username or Email already exists");

        var (hash, salt) = PasswordHasher.HashPassword(dto.Password);
        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = hash,
            Salt = salt,
        };

        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
        if (role is null)
        {
            role = new Role { Name = roleName, Description = $"{roleName} role" };
            _db.Roles.Add(role);
            await _db.SaveChangesAsync();
        }

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetUser),
            new { id = user.Id },
            new { user.Id, user.Username }
        );
    }

    [Authorize]
    [HttpGet("getuser")]
    public async Task<ActionResult<UserReadDto>> GetUser()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!int.TryParse(sub, out var userId))
            return Unauthorized();
        var user = await _db
            .Users.Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
            return NotFound();
        var dto = new UserReadDto(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Username,
            user.Email,
            user.UserRoles.Select(r => r.Role!.Name)
        );
        return Ok(dto);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("getallusers")]
    public async Task<ActionResult<IEnumerable<UserReadDto>>> GetAllUsers()
    {
        var users = await _db
            .Users.Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ToListAsync();
        var list = users.Select(u => new UserReadDto(
            u.Id,
            u.FirstName,
            u.LastName,
            u.Username,
            u.Email,
            u.UserRoles.Select(r => r.Role!.Name)
        ));
        return Ok(list);
    }

    [Authorize]
    [HttpPost("update")]
    public async Task<ActionResult> Update(UpdateUserDto dto)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!int.TryParse(sub, out var userId))
            return Unauthorized();
        var user = await _db.Users.FindAsync(userId);
        if (user is null)
            return NotFound();
        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.Email = dto.Email;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("delete")]
    public async Task<ActionResult> Delete([FromQuery] int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null)
            return NotFound();
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
