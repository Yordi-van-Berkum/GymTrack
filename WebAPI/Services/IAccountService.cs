using System.Security.Claims;
using WebAPI.Models.Auth;

namespace WebAPI.Services
{
    public interface IAccountService
    {
        Task<UserProfile?> GetMyProfileAsync(Guid userId, CancellationToken cancellationToken = default);
        Task UpdateMyProfileAsync(Guid userId, UserProfileDto userProfileDto, CancellationToken cancellationToken = default);
    }
}