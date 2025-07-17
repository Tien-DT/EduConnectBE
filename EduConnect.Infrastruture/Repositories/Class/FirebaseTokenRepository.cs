using EduConnect.Data;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace EduConnect.Infrastruture.Repositories.Class
{
    public class FirebaseTokenRepository : BaseRepository<FirebaseToken, int>, IFirebaseTokenRepository
    {
        public FirebaseTokenRepository(EduConnectDbContext context) : base(context)
        {
        }

        public async Task<FirebaseToken?> GetActiveTokenByUserIdAsync(string userId, string? deviceId = null)
        {
            var query = _context.FirebaseTokens
                .Where(ft => ft.UserId == userId && ft.IsActive);

            if (!string.IsNullOrEmpty(deviceId))
            {
                query = query.Where(ft => ft.DeviceId == deviceId);
            }

            return await query
                .OrderByDescending(ft => ft.UpdatedAt ?? ft.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<List<FirebaseToken>> GetActiveTokensByUserIdAsync(string userId)
        {
            return await _context.FirebaseTokens
                .Where(ft => ft.UserId == userId && ft.IsActive)
                .OrderByDescending(ft => ft.UpdatedAt ?? ft.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> DeactivateTokenAsync(int tokenId)
        {
            var token = await _context.FirebaseTokens.FindAsync(tokenId);
            if (token == null) return false;

            token.IsActive = false;
            token.UpdatedAt = DateTime.Now;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeactivateAllUserTokensAsync(string userId)
        {
            var tokens = await _context.FirebaseTokens
                .Where(ft => ft.UserId == userId && ft.IsActive)
                .ToListAsync();

            if (!tokens.Any()) return true;

            foreach (var token in tokens)
            {
                token.IsActive = false;
                token.UpdatedAt = DateTime.Now;
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateTokenAsync(string userId, string newToken, string? deviceType = null, string? deviceId = null)
        {
            var existingToken = await GetActiveTokenByUserIdAsync(userId, deviceId);

            if (existingToken != null)
            {
                existingToken.Token = newToken;
                existingToken.UpdatedAt = DateTime.Now;
                existingToken.ExpiresAt = DateTime.Now.AddDays(60); // Token expires in 60 days
                
                if (!string.IsNullOrEmpty(deviceType))
                    existingToken.DeviceType = deviceType;
            }
            else
            {
                var newFirebaseToken = new FirebaseToken
                {
                    UserId = userId,
                    Token = newToken,
                    DeviceType = deviceType,
                    DeviceId = deviceId,
                    ExpiresAt = DateTime.Now.AddDays(60),
                    IsActive = true
                };

                await _context.FirebaseTokens.AddAsync(newFirebaseToken);
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<FirebaseToken> CreateOrUpdateTokenAsync(string userId, string token, string? deviceType = null, string? deviceId = null)
        {
            // First deactivate old tokens for the same device
            if (!string.IsNullOrEmpty(deviceId))
            {
                var oldTokens = await _context.FirebaseTokens
                    .Where(ft => ft.UserId == userId && ft.DeviceId == deviceId && ft.IsActive)
                    .ToListAsync();

                foreach (var oldToken in oldTokens)
                {
                    oldToken.IsActive = false;
                    oldToken.UpdatedAt = DateTime.Now;
                }
            }

            // Create new token
            var newToken = new FirebaseToken
            {
                UserId = userId,
                Token = token,
                DeviceType = deviceType,
                DeviceId = deviceId,
                ExpiresAt = DateTime.Now.AddDays(60),
                IsActive = true
            };

            await _context.FirebaseTokens.AddAsync(newToken);
            await _context.SaveChangesAsync();

            return newToken;
        }

        public async Task<bool> CleanupExpiredTokensAsync()
        {
            var expiredTokens = await _context.FirebaseTokens
                .Where(ft => ft.ExpiresAt.HasValue && ft.ExpiresAt < DateTime.Now)
                .ToListAsync();

            if (!expiredTokens.Any()) return true;

            foreach (var token in expiredTokens)
            {
                token.IsActive = false;
                token.UpdatedAt = DateTime.Now;
            }

            return await _context.SaveChangesAsync() > 0;
        }
    }
}
