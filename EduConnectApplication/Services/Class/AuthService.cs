using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using EduConnect.Application.Services.Interface;
using EduConnect.Domain.Context;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Repositories.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EduConnect.Application.Services.Class
{
        public class AuthService : IAuthService
        {
            private readonly IAuthRepository _authRepository;
            private readonly IConfiguration _configuration;

            public AuthService(IAuthRepository authRepository, IConfiguration configuration)
            {
                _authRepository = authRepository;
                _configuration = configuration;
            }

            public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest login)
            {
                var account = await _authRepository.GetAccountByEmailAsync(login.Email);

                if (account is null)
                {
                    return ApiResponse<LoginResponse>.ErrorResponse(
                        new List<string> { "Email not exist." },
                        "Login Failed"
                    );
                }

                var passwordHasher = new PasswordHasher<Account>();
                var result = passwordHasher.VerifyHashedPassword(account, account.PasswordHash, login.Password);

                if (result == PasswordVerificationResult.Failed)
                {
                    return ApiResponse<LoginResponse>.ErrorResponse(
                        new List<string> { "Password Incorrect" },
                        "Login Failed"
                    );
                }

                string token = GenerateToken(account);
                string refreshToken = await SaveRefreshToken(account);
                return ApiResponse<LoginResponse>.SuccessResponse(new LoginResponse
                {
                    Token = token,
                    RefreshToken = refreshToken
                }, "Login Succeeded");
            }

            public async Task<ApiResponse<RegisterResponse>> RegisterAsync(RegisterRequest register)
            {
                if (await _authRepository.EmailExistsAsync(register.Email))
                {
                    return ApiResponse<RegisterResponse>.ErrorResponse(
                        new List<string> { "Email already in use." },
                        "Registration Failed"
                    );
                }

                var account = new Account();
                var hashedPassword = new PasswordHasher<Account>().HashPassword(account, register.Password);
                account.AccountId = Guid.NewGuid();
                account.Email = register.Email;
                account.FullName = register.FullName;
                account.PasswordHash = hashedPassword;
                account.RoleId = register.Role;

                await _authRepository.AddAccountAsync(account);
                await _authRepository.SaveChangesAsync();

                var responseDTO = new RegisterResponse
                {
                    Email = account.Email,
                    RoleId = account.RoleId.Value
                };

                return ApiResponse<RegisterResponse>.SuccessResponse(responseDTO, "Registration Succeeded");
            }

            private string GenerateToken(Account account)
            {
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, account.AccountId.ToString()),
                new Claim(ClaimTypes.Name, account.FullName),
                new Claim(ClaimTypes.Email, account.Email),
                new Claim(ClaimTypes.Role, account.RoleId.ToString())
            };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var tokenDescriptor = new JwtSecurityToken(
                    issuer: _configuration["JwtSettings:Issuer"],
                    audience: _configuration["JwtSettings:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(24),
                    signingCredentials: creds
                );

                return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
            }

            private string GenerateRefreshToken()
            {
                var randomNumber = new byte[32];
                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }

            private async Task<string> SaveRefreshToken(Account account)
            {
                var refreshToken = GenerateRefreshToken();
                await _authRepository.UpdateRefreshTokenAsync(account, refreshToken, DateTime.UtcNow.AddDays(7));
                return refreshToken;
            }

            private async Task<Account?> ValidateRefreshTokenAsync(Guid accountId, string refreshToken)
            {
                var account = await _authRepository.GetAccountByIdAsync(accountId);
                if (account is null || account.RefreshToken != refreshToken || account.RefreshTokenExpTime < DateTime.UtcNow)
                {
                    return null;
                }
                return account;
            }

            public async Task<GetNewTokenResponse> GetNewToken(GetNewTokenRequest request)
            {
                var account = await ValidateRefreshTokenAsync(request.AccountId, request.RefreshToken);
                if (account is null)
                {
                    return null;
                }
                GetNewTokenResponse response = new GetNewTokenResponse
                {
                    AccessToken = GenerateToken(account),
                    RefreshToken = await SaveRefreshToken(account)
                };
                return response;
            }
        }
    }



