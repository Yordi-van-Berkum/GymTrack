using System.Security.Claims;
using WebAPI.Models.Auth;

namespace WebAPI.Services
{
    public interface IAccountService
    {
        Task<UserProfile?> GetMyProfileAsync(ClaimsPrincipal user);
    }
}