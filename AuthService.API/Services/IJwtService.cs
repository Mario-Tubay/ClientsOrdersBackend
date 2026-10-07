using AuthService.API.Models;

namespace AuthService.API.Services;

public interface IJwtService
{
    string GenerateToken(User user);
}
