using EduConnect.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace EduConnect.Infrastruture.Repositories.Interface
{
    public interface IAuthRepository
    {
        Task<ApplicationUser?> FindByEmailAsync(string email);
        Task<ApplicationUser?> GetUserByIdAsync(string userId);
        Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password);
        Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
        Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string roleId);
        Task<IdentityResult> AddToRoleByNameAsync(ApplicationUser user, string roleName);
        Task<IList<string>> GetRolesAsync(ApplicationUser user);
        Task SaveRefreshTokenAsync(string userId, string refreshToken, DateTime expTime);
        Task<ApplicationUser?> GetUserWithRolesAsync(string userId);
        Task<RefreshToken?> GetRefreshTokenAsync(string userId, string refreshToken);
        Task RemoveRefreshTokenAsync(string userId);
        Task<bool> IsEmailExistsAsync(string email);
        Task<bool> IsRefreshTokenValidAsync(string userId, string refreshToken);
    }
}