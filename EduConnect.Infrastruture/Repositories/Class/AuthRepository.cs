using EduConnect.Data;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Repositories.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduConnect.Infrastruture.Repositories.Class
{
    public class AuthRepository : IAuthRepository
    {
        private readonly EduConnectDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthRepository(
            EduConnectDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        public async Task<ApplicationUser?> FindByEmailAsync(string email)
        {
            return await _userManager.Users
                
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
        {
            return await _userManager.Users
                
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<ApplicationUser?> GetUserWithRolesAsync(string userId)
        {
            return await _userManager.Users
                
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        public async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }

        public async Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "RoleNotFound",
                    Description = $"Role with ID '{roleId}' was not found."
                });
            }

            return await _userManager.AddToRoleAsync(user, role.Name);
        }

        public async Task<IdentityResult> AddToRoleByNameAsync(ApplicationUser user, string roleName)
        {
            return await _userManager.AddToRoleAsync(user, roleName);
        }

        public async Task<IList<string>> GetRolesAsync(ApplicationUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        public async Task SaveRefreshTokenAsync(string userId, string refreshToken, DateTime expTime)
        {
            var existingToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.UserId == userId);

            if (existingToken != null)
            {
                existingToken.RefreshTokenValue = refreshToken;
                existingToken.ExpirationTime = expTime;
                _context.RefreshTokens.Update(existingToken);
            }
            else
            {
                var newRefreshToken = new RefreshToken
                {
                    UserId = userId,
                    RefreshTokenValue = refreshToken,
                    ExpirationTime = expTime
                };

                await _context.RefreshTokens.AddAsync(newRefreshToken);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string userId, string refreshToken)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.UserId == userId
                    && rt.RefreshTokenValue == refreshToken
                    && rt.ExpirationTime > DateTime.UtcNow);
        }

        public async Task<bool> IsRefreshTokenValidAsync(string userId, string refreshToken)
        {
            var token = await GetRefreshTokenAsync(userId, refreshToken);
            return token != null;
        }

        public async Task RemoveRefreshTokenAsync(string userId)
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.UserId == userId);

            if (refreshToken != null)
            {
                _context.RefreshTokens.Remove(refreshToken);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user != null;
        }

        public async Task<ApplicationUser?> FindByGoogleIdAsync(string googleId)
        {
            return await _userManager.Users
                .FirstOrDefaultAsync(u => u.GoogleId == googleId);
        }

        public async Task<IdentityResult> UpdateUserAsync(ApplicationUser user)
        {
            return await _userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> CreateUserWithoutPasswordAsync(ApplicationUser user)
        {
            return await _userManager.CreateAsync(user);
        }
    }
}