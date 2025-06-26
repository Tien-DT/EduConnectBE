using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using EduConnect.Application.Services.Interface;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Repositories.Interface;
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
using EduConnect.Domain.Enums;

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
            try
            {
                if (string.IsNullOrWhiteSpace(login.Email) || string.IsNullOrWhiteSpace(login.Password))
                {
                    return ApiResponse<LoginResponse>.ErrorResponse(
                        new List<string> { "Email and password are required." },
                        "Login Failed"
                    );
                }
                var account = await _authRepository.FindByEmailAsync(login.Email);

                if (account is null)
                {
                    return ApiResponse<LoginResponse>.ErrorResponse(
                        new List<string> { "Email does not exist." },
                        "Login Failed"
                    );
                }

                if (!account.IsActive)
                {
                    return ApiResponse<LoginResponse>.ErrorResponse(
                        new List<string> { "Account is not active." },
                        "Login Failed"
                    );
                }
                if (account.LockoutEnd.HasValue && account.LockoutEnd > DateTimeOffset.UtcNow)
                {
                    return ApiResponse<LoginResponse>.ErrorResponse(
                        new List<string> { "Account is temporarily locked. Please try again later." },
                        "Login Failed"
                    );
                }
                var result = await _authRepository.CheckPasswordAsync(account, login.Password);

                if (!result)
                {
                    return ApiResponse<LoginResponse>.ErrorResponse(
                        new List<string> { "Incorrect password." },
                        "Login Failed"
                    );
                }

                string token = await GenerateToken(account);
                string refreshToken = await SaveRefreshToken(account);

                var response = new LoginResponse
                {
                    Token = token,
                    RefreshToken = refreshToken
                };

                return ApiResponse<LoginResponse>.SuccessResponse(response, "Login Succeeded");
            }
            catch (Exception ex)
            {
                return ApiResponse<LoginResponse>.ErrorResponse(
                    new List<string> { "An error occurred during login. Please try again." },
                    "Login Failed"
                );
            }
        }

        public async Task<ApiResponse<RegisterResponse>> RegisterAsync(RegisterRequest register)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(register.Email))
                {
                    return ApiResponse<RegisterResponse>.ErrorResponse(
                        new List<string> { "Email cannot be null or empty." },
                        "Registration Failed"
                    );
                }

                if (string.IsNullOrWhiteSpace(register.Password))
                {
                    return ApiResponse<RegisterResponse>.ErrorResponse(
                        new List<string> { "Password cannot be null or empty." },
                        "Registration Failed"
                    );
                }

                if (register.Password != register.ConfirmPassword)
                {
                    return ApiResponse<RegisterResponse>.ErrorResponse(
                        new List<string> { "Password and confirm password do not match." },
                        "Registration Failed"
                    );
                }

                if (string.IsNullOrWhiteSpace(register.FirstName) || string.IsNullOrWhiteSpace(register.LastName))
                {
                    return ApiResponse<RegisterResponse>.ErrorResponse(
                        new List<string> { "First name and last name are required." },
                        "Registration Failed"
                    );
                }

                if (!register.Role.HasValue)
                {
                    return ApiResponse<RegisterResponse>.ErrorResponse(
                        new List<string> { "Role is required." },
                        "Registration Failed"
                    );
                }

                var existingAccount = await _authRepository.FindByEmailAsync(register.Email);
                if (existingAccount != null)
                {
                    return ApiResponse<RegisterResponse>.ErrorResponse(
                        new List<string> { "Email already in use." },
                        "Registration Failed"
                    );
                }

                var accountRegister = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = register.Email,
                    UserName = register.Email,
                    NormalizedEmail = register.Email.ToUpperInvariant(),
                    NormalizedUserName = register.Email.ToUpperInvariant(),
                    FirstName = register.FirstName,
                    LastName = register.LastName,
                    EmailConfirmed = false,
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                    LockoutEnabled = true,
                    AccessFailedCount = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    AvatarUrl = string.Empty
                };

                var createResult = await _authRepository.CreateUserAsync(accountRegister, register.Password);

                if (!createResult.Succeeded)
                {
                    return ApiResponse<RegisterResponse>.ErrorResponse(
                        createResult.Errors.Select(e => e.Description).ToList(),
                        "Registration Failed"
                    );
                }

                var roleName = register.Role.ToString();
                var roleResult = await _authRepository.AddToRoleAsync(accountRegister, roleName);
                if (!roleResult.Succeeded)
                {
                    return ApiResponse<RegisterResponse>.ErrorResponse(
                        roleResult.Errors.Select(e => e.Description).ToList(),
                        "Role Assignment Failed"
                    );
                }

                var responseDTO = new RegisterResponse
                {
                    Email = accountRegister.Email,
                    FirstName = accountRegister.FirstName,
                    LastName = accountRegister.LastName,
                };

                return ApiResponse<RegisterResponse>.SuccessResponse(responseDTO, "Registration Succeeded");
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += " | Inner exception: " + ex.InnerException.Message;
                }

                return ApiResponse<RegisterResponse>.ErrorResponse(
                    new List<string> { errorMessage },
                    "Registration Failed"
                );
            }
        }

        public async Task<ApiResponse<GetNewTokenResponse>> GetNewTokenAsync(GetNewTokenRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.AccountId) || string.IsNullOrWhiteSpace(request.RefreshToken))
                {
                    return ApiResponse<GetNewTokenResponse>.ErrorResponse(
                        new List<string> { "User ID and refresh token are required." },
                        "Token Refresh Failed"
                    );
                }
                var account = await ValidateRefreshTokenAsync(request.AccountId, request.RefreshToken);
                if (account is null)
                {
                    return ApiResponse<GetNewTokenResponse>.ErrorResponse(
                        new List<string> { "Invalid or expired refresh token." },
                        "Token Refresh Failed"
                    );
                }
                if (!account.IsActive)
                {
                    return ApiResponse<GetNewTokenResponse>.ErrorResponse(
                        new List<string> { "Account is not active." },
                        "Token Refresh Failed"
                    );
                }
                var response = new GetNewTokenResponse
                {
                    AccessToken = await GenerateToken(account),
                    RefreshToken = await SaveRefreshToken(account)
                };

                return ApiResponse<GetNewTokenResponse>.SuccessResponse(response, "Token refreshed successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<GetNewTokenResponse>.ErrorResponse(
                    new List<string> { "An error occurred during token refresh. Please try again." },
                    "Token Refresh Failed"
                );
            }
        }

        private async Task<string> GenerateToken(ApplicationUser account)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, account.Id),
                new Claim(ClaimTypes.Name, $"{account.FirstName} {account.LastName}"),
                new Claim(ClaimTypes.Email, account.Email),
                new Claim("avatar_url", account.AvatarUrl ?? string.Empty)
            };

            var roles = await _authRepository.GetRolesAsync(account);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var currentVietnamTime = TimeZoneInfo.ConvertTime(DateTime.Now, vietnamTimeZone);
            var expireVietnamTime = currentVietnamTime.AddHours(24);
            var expireUtcTime = TimeZoneInfo.ConvertTimeToUtc(expireVietnamTime, vietnamTimeZone);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: expireUtcTime,
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

        private async Task<string> SaveRefreshToken(ApplicationUser account)
        {
            var refreshToken = GenerateRefreshToken();
            await _authRepository.SaveRefreshTokenAsync(account.Id, refreshToken, DateTime.UtcNow.AddDays(7));
            return refreshToken;
        }

        private async Task<ApplicationUser?> ValidateRefreshTokenAsync(string userId, string refreshToken)
        {
            var isValid = await _authRepository.IsRefreshTokenValidAsync(userId, refreshToken);
            if (!isValid)
            {
                return null;
            }

            return await _authRepository.GetUserByIdAsync(userId);
        }

        public async Task<ApiResponse<LoginResponse>> GoogleLoginAsync(GoogleLoginRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.GoogleId) || string.IsNullOrWhiteSpace(request.Email))
                {
                    return ApiResponse<LoginResponse>.ErrorResponse(
                        new List<string> { "Google ID and email are required." },
                        "Google Login Failed"
                    );
                }
                var existingUser = await _authRepository.FindByEmailAsync(request.Email);
                
                if (existingUser != null)
                {
                    existingUser.GoogleId = request.GoogleId;
                    existingUser.AvatarUrl = request.Picture; 
                    existingUser.UpdatedAt = DateTime.UtcNow;
                    var updateResult = await _authRepository.UpdateUserAsync(existingUser);
                    if (!updateResult.Succeeded)
                    {
                        return ApiResponse<LoginResponse>.ErrorResponse(
                            new List<string> { "Failed to link Google account." },
                            "Google Login Failed"
                        );
                    }
                }
                else
                {
                    existingUser = new ApplicationUser
                    {
                        Id = Guid.NewGuid().ToString(),
                        Email = request.Email,
                        UserName = request.Email,
                        NormalizedEmail = request.Email.ToUpperInvariant(),
                        NormalizedUserName = request.Email.ToUpperInvariant(),
                        FirstName = request.GivenName,
                        LastName = request.FamilyName,
                        GoogleId = request.GoogleId,
                        AvatarUrl = request.Picture,
                        EmailConfirmed = request.EmailVerified,
                        PhoneNumberConfirmed = false,
                        TwoFactorEnabled = false,
                        LockoutEnabled = false,
                        AccessFailedCount = 0,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        SecurityStamp = Guid.NewGuid().ToString(),
                        ConcurrencyStamp = Guid.NewGuid().ToString()
                    };
                    var createResult = await _authRepository.CreateUserWithoutPasswordAsync(existingUser);
                    if (!createResult.Succeeded)
                    {
                        return ApiResponse<LoginResponse>.ErrorResponse(
                            createResult.Errors.Select(e => e.Description).ToList(),
                            "Google Login Failed"
                        );
                    }
                    var roleResult = await _authRepository.AddToRoleAsync(existingUser, "0");
                    if (!roleResult.Succeeded)
                    {
                        return ApiResponse<LoginResponse>.ErrorResponse(
                            new List<string> { "Failed to assign default role." },
                            "Google Login Failed"
                        );
                    }
                    existingUser = await _authRepository.GetUserByIdAsync(existingUser.Id);
                }
                if (!existingUser.IsActive)
                {
                    return ApiResponse<LoginResponse>.ErrorResponse(
                        new List<string> { "Account is not active." },
                        "Google Login Failed"
                    );
                }
                string token = await GenerateToken(existingUser);
                string refreshToken = await SaveRefreshToken(existingUser);

                var response = new LoginResponse
                {
                    Token = token,
                    RefreshToken = refreshToken
                };

                return ApiResponse<LoginResponse>.SuccessResponse(response, "Google Login Succeeded");
            }
            catch (Exception ex)
            {
                return ApiResponse<LoginResponse>.ErrorResponse(
                    new List<string> { "An error occurred during Google login. Please try again." },
                    "Google Login Failed"
                );
            }
        }
    }
}