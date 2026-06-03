using System.Security.Claims;
using BookingSystem.Enums;
using Microsoft.IdentityModel.JsonWebTokens;

namespace BookingSystem.Helper;

public static class ClaimToken
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? throw new UnauthorizedAccessException("User ID claim is missing.");

        return Guid.Parse(value);
    }

    public static UserRole GetRole(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.Role)
            ?? throw new UnauthorizedAccessException("Role claim is missing.");

        return Enum.Parse<UserRole>(value);
    }
}
