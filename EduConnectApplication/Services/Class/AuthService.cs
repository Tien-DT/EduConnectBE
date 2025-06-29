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
using Google.Apis.Auth;

namespace EduConnect.Application.Services.Class
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;
        private readonly ITeacherRepository _teacherRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IParentRepository _parentRepository;
        private readonly IPrincipalRepository _principalRepository;
        private readonly IEmailService _emailService;
        private readonly IEmailVerificationTokenService _emailVerificationTokenService;

        public AuthService(IAuthRepository authRepository, IConfiguration configuration,
            ITeacherRepository teacherRepository, IStudentRepository studentRepository,
            IParentRepository parentRepository, IPrincipalRepository principalRepository,
            IEmailService emailService, IEmailVerificationTokenService emailVerificationTokenService)
        {
            _authRepository = authRepository;
            _configuration = configuration;
            _teacherRepository = teacherRepository;
            _studentRepository = studentRepository;
            _parentRepository = parentRepository;
            _principalRepository = principalRepository;
            _emailService = emailService;
            _emailVerificationTokenService = emailVerificationTokenService;
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
            catch (Exception)
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
                var roleResult = await _authRepository.AddToRoleAsync(accountRegister, "0");
                if (!roleResult.Succeeded)
                {
                    return ApiResponse<RegisterResponse>.ErrorResponse(
                        roleResult.Errors.Select(e => e.Description).ToList(),
                        "Role Assignment Failed"
                    );
                }

                try
                {
                    Console.WriteLine($"DEBUG: About to generate token for user {accountRegister.Id} with email {accountRegister.Email}");
                    var verificationToken = await _emailVerificationTokenService.GenerateAndStoreTokenAsync(accountRegister.Id, accountRegister.Email);
                    Console.WriteLine($"DEBUG: Generated verification token: {verificationToken ?? "NULL"}");
                    
                    if (string.IsNullOrEmpty(verificationToken))
                    {
                        Console.WriteLine("ERROR: Verification token is null or empty!");
                        return ApiResponse<RegisterResponse>.ErrorResponse(
                            new List<string> { "Failed to generate verification token. Please try again." },
                            "Registration Failed"
                        );
                    }
                    
                    try
                    {
                        await _emailService.SendVerificationEmailAsync(accountRegister.Email, verificationToken, accountRegister.Id);
                        Console.WriteLine("DEBUG: Verification email sent successfully");
                    }
                    catch (Exception emailEx)
                    {
                        Console.WriteLine($"Failed to send verification email: {emailEx.Message}");
                    }
                }
                catch (Exception tokenEx)
                {
                    Console.WriteLine($"ERROR: Exception during token generation: {tokenEx.Message}");
                    Console.WriteLine($"Stack trace: {tokenEx.StackTrace}");
                    return ApiResponse<RegisterResponse>.ErrorResponse(
                        new List<string> { "Failed to generate verification token. Please try again." },
                        "Registration Failed"
                    );
                }

                var responseDTO = new RegisterResponse
                {
                    Email = accountRegister.Email,
                    FirstName = accountRegister.FirstName,
                    LastName = accountRegister.LastName,
                };

                return ApiResponse<RegisterResponse>.SuccessResponse(responseDTO, "Registration succeeded. Please check your email for verification link.");
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
            catch (Exception)
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
                new Claim(ClaimTypes.Email, account.Email ?? string.Empty)
            };
            if (!string.IsNullOrWhiteSpace(account.AvatarUrl))
            {
                claims.Add(new Claim("avatar_url", account.AvatarUrl));
            }

            var roles = await _authRepository.GetRolesAsync(account);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var currentVietnamTime = TimeZoneInfo.ConvertTime(DateTime.Now, vietnamTimeZone);
            var expireVietnamTime = currentVietnamTime.AddSeconds(15);
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
                        AvatarUrl = !string.IsNullOrWhiteSpace(request.Picture) ? request.Picture : string.Empty,
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
                if (existingUser == null)
                {
                    return ApiResponse<LoginResponse>.ErrorResponse(
                        new List<string> { "Failed to retrieve user information." },
                        "Google Login Failed"
                    );
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
            catch (Exception)
            {
                return ApiResponse<LoginResponse>.ErrorResponse(
                    new List<string> { "An error occurred during Google login. Please try again." },
                    "Google Login Failed"
                );
            }
        }

        public async Task<ApiResponse<LoginResponse>> GoogleTokenLoginAsync(GoogleTokenLoginRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.GoogleToken))
                {
                    return ApiResponse<LoginResponse>.ErrorResponse(
                        new List<string> { "Google token is required." },
                        "Google Token Login Failed"
                    );
                }
                GoogleJsonWebSignature.Payload payload;
                try
                {
                    payload = await GoogleJsonWebSignature.ValidateAsync(request.GoogleToken);
                }
                catch (Exception ex)
                {
                    return ApiResponse<LoginResponse>.ErrorResponse(
                        new List<string> { "Invalid Google token.", ex.Message },
                        "Google Token Login Failed"
                    );
                }
                var email = payload.Email;
                var googleId = payload.Subject; 
                var givenName = payload.GivenName ?? string.Empty;
                var familyName = payload.FamilyName ?? string.Empty;
                var picture = payload.Picture ?? string.Empty;
                var emailVerified = payload.EmailVerified;

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(googleId))
                {
                    return ApiResponse<LoginResponse>.ErrorResponse(
                        new List<string> { "Invalid token payload: missing email or user ID." },
                        "Google Token Login Failed"
                    );
                }
                var existingUser = await _authRepository.FindByEmailAsync(email);
                
                if (existingUser != null)
                {
                    if (string.IsNullOrWhiteSpace(existingUser.GoogleId))
                    {
                        existingUser.GoogleId = googleId;
                        existingUser.UpdatedAt = DateTime.UtcNow;
                        var updateResult = await _authRepository.UpdateUserAsync(existingUser);
                        if (!updateResult.Succeeded)
                        {
                            return ApiResponse<LoginResponse>.ErrorResponse(
                                new List<string> { "Failed to link Google account." },
                                "Google Token Login Failed"
                            );
                        }
                    }
                }
                else
                {
                    existingUser = new ApplicationUser
                    {
                        Id = Guid.NewGuid().ToString(),
                        Email = email,
                        UserName = email,
                        NormalizedEmail = email.ToUpperInvariant(),
                        NormalizedUserName = email.ToUpperInvariant(),
                        FirstName = givenName,
                        LastName = familyName,
                        GoogleId = googleId,
                        AvatarUrl = !string.IsNullOrWhiteSpace(picture) ? picture : string.Empty,
                        EmailConfirmed = emailVerified,
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
                            "Google Token Login Failed"
                        );
                    }
                    var roleResult = await _authRepository.AddToRoleByNameAsync(existingUser, "Student");
                    if (!roleResult.Succeeded)
                    {
                        return ApiResponse<LoginResponse>.ErrorResponse(
                            new List<string> { "Failed to assign default role." },
                            "Google Token Login Failed"
                        );
                    }

                    existingUser = await _authRepository.GetUserByIdAsync(existingUser.Id);
                }

                if (existingUser == null)
                {
                    return ApiResponse<LoginResponse>.ErrorResponse(
                        new List<string> { "Failed to retrieve user information." },
                        "Google Token Login Failed"
                    );
                }
                
                if (!existingUser.IsActive)
                {
                    return ApiResponse<LoginResponse>.ErrorResponse(
                        new List<string> { "Account is not active." },
                        "Google Token Login Failed"
                    );
                }
                string token = await GenerateToken(existingUser);
                string refreshToken = await SaveRefreshToken(existingUser);

                var response = new LoginResponse
                {
                    Token = token,
                    RefreshToken = refreshToken
                };

                return ApiResponse<LoginResponse>.SuccessResponse(response, "Google Token Login Succeeded");
            }
            catch (Exception ex)
            {
                return ApiResponse<LoginResponse>.ErrorResponse(
                    new List<string> { "An error occurred during Google token login.", ex.Message },
                    "Google Token Login Failed"
                );
            }
        }

        public async Task<ApiResponse<UserRoleIdResponse>> GetUserRoleIdAsync(string userId)
        {
            try
            {
                var user = await _authRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<UserRoleIdResponse>.ErrorResponse(
                        new List<string> { "User not found." },
                        "User Not Found"
                    );
                }

                var roles = await _authRepository.GetRolesAsync(user);
                if (roles == null || !roles.Any())
                {
                    return ApiResponse<UserRoleIdResponse>.ErrorResponse(
                        new List<string> { "User has no assigned roles." },
                        "No Roles Found"
                    );
                }

                var role = roles.First(); 
                string roleId = string.Empty;
                string fullName = string.Empty;

                switch (role.ToLower())
                {
                    case "teacher":
                        var teachers = await _teacherRepository.GetAllAsync();
                        var teacher = teachers.FirstOrDefault(t => t.AccountID == userId);
                        if (teacher != null)
                        {
                            roleId = teacher.TeacherID;
                            fullName = teacher.FullName ?? string.Empty;
                        }
                        break;
                    case "student":
                        var students = await _studentRepository.GetAllAsync();
                        var student = students.FirstOrDefault(s => s.AccountID == userId);
                        if (student != null)
                        {
                            roleId = student.StudentID;
                            fullName = student.FullName ?? string.Empty;
                        }
                        break;
                    case "parent":
                        var parents = await _parentRepository.GetAllAsync();
                        var parent = parents.FirstOrDefault(p => p.AccountID == userId);
                        if (parent != null)
                        {
                            roleId = parent.ParentID;
                            fullName = parent.FullName ?? string.Empty;
                        }
                        break;
                    case "principal":
                        if (user.Principal != null)
                        {
                            roleId = user.Principal.PrincipalID;
                            fullName = user.Principal.FullName ?? string.Empty;
                        }
                        break;
                    default:
                        return ApiResponse<UserRoleIdResponse>.ErrorResponse(
                            new List<string> { "Invalid role type." },
                            "Invalid Role"
                        );
                }

                if (string.IsNullOrEmpty(roleId))
                {
                    return ApiResponse<UserRoleIdResponse>.ErrorResponse(
                        new List<string> { $"No {role} profile found for this user." },
                        "Profile Not Found"
                    );
                }

                var response = new UserRoleIdResponse
                {
                    UserId = userId,
                    Role = role,
                    Id = roleId,
                    FullName = fullName
                };

                return ApiResponse<UserRoleIdResponse>.SuccessResponse(response, "User role ID retrieved successfully");
            }
            catch (Exception)
            {
                return ApiResponse<UserRoleIdResponse>.ErrorResponse(
                    new List<string> { "An error occurred while retrieving user role ID. Please try again." },
                    "Get User Role ID Failed"
                );
            }
        }

        public async Task<ApiResponse<string>> VerifyEmailAsync(EmailVerificationRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Token))
                {
                    return ApiResponse<string>.ErrorResponse(
                        new List<string> { "Email and token are required." },
                        "Email Verification Failed"
                    );
                }
                var userId = await _emailVerificationTokenService.ValidateAndConsumeTokenAsync(request.Token);
                if (userId == null)
                {
                    return ApiResponse<string>.ErrorResponse(
                        new List<string> { "Invalid or expired verification token." },
                        "Email Verification Failed"
                    );
                }
                var user = await _authRepository.FindByEmailAsync(request.Email);
                if (user == null || user.Id != userId)
                {
                    return ApiResponse<string>.ErrorResponse(
                        new List<string> { "Invalid verification request." },
                        "Email Verification Failed"
                    );
                }
                if (user.EmailConfirmed)
                {
                    return ApiResponse<string>.SuccessResponse("Email already verified", "Email Already Verified");
                }
                user.EmailConfirmed = true;
                user.UpdatedAt = DateTime.UtcNow;
                
                var updateResult = await _authRepository.UpdateUserAsync(user);
                if (!updateResult.Succeeded)
                {
                    return ApiResponse<string>.ErrorResponse(
                        updateResult.Errors.Select(e => e.Description).ToList(),
                        "Email Verification Failed"
                    );
                }

                return ApiResponse<string>.SuccessResponse("Email verified successfully", "Email Verification Succeeded");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.ErrorResponse(
                    new List<string> { "An error occurred during email verification.", ex.Message },
                    "Email Verification Failed"
                );
            }
        }

    }
}