using System.Security.Claims;

namespace WebAPI.Extensions
{
    public static class UserExtensions
    {
        public static bool TryGetUserId(this ClaimsPrincipal user, out Guid userId)
        {
            var value = user.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(value, out userId);
        }
    }
}
