using EduConnect.Domain.Entities;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EduConnect.Infrastruture.Repositories.Interface
{
    public interface IAuthRepository
    {
        Task<Account?> GetAccountByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task<Account?> GetAccountByIdAsync(Guid accountId);
        Task AddAccountAsync(Account account);
        Task SaveChangesAsync();
        Task UpdateRefreshTokenAsync(Account account, string refreshToken, DateTime expiryTime);
    }
}