namespace AuthService.Contracts;

public record RegisterDto(
    string FirstName,
    string LastName,
    string Username,
    string Email,
    string Password
);

public record LoginDto(string UsernameOrEmail, string Password);

public record UpdateUserDto(string FirstName, string LastName, string Email);

public record UserReadDto(
    int Id,
    string FirstName,
    string LastName,
    string Username,
    string Email,
    IEnumerable<string> Roles
);
