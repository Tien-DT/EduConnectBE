using EduConnect.Domain.Context;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EduConnect.Infrastruture.Repositories.Class
{
        public class AuthRepository : IAuthRepository
        {
            private readonly EduConnectDbContext _context;

            public AuthRepository(EduConnectDbContext context)
            {
                _context = context;
            }

            public async Task<Account?> GetAccountByEmailAsync(string email)
            {
                return await _context.Accounts
                    .FirstOrDefaultAsync(a => a.Email == email);
            }

            public async Task<bool> EmailExistsAsync(string email)
            {
                return await _context.Accounts.AnyAsync(a => a.Email == email);
            }

            public async Task<Account?> GetAccountByIdAsync(Guid accountId)
            {
                return await _context.Accounts.FindAsync(accountId);
            }

            public async Task AddAccountAsync(Account account)
            {
                await _context.Accounts.AddAsync(account);
            }

            public async Task SaveChangesAsync()
            {
                await _context.SaveChangesAsync();
            }

            public async Task UpdateRefreshTokenAsync(Account account, string refreshToken, DateTime expiryTime)
            {
                account.RefreshToken = refreshToken;
                account.RefreshTokenExpTime = expiryTime;
                await SaveChangesAsync();
            }
        }
    }
