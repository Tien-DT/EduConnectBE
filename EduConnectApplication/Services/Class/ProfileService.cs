using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using EduConnect.Application.Services.Interface;
using EduConnect.Infrastruture.Repositories.Interface;
using EduConnectApplication.Services.Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduConnect.Application.Services.Class
{
    public class ProfileService : IProfileService
    {
        private readonly IAuthRepository _authRepository;
        private readonly FileUploadService _fileUploadService;

        public ProfileService(IAuthRepository authRepository, FileUploadService fileUploadService)
        {
            _authRepository = authRepository;
            _fileUploadService = fileUploadService;
        }

        public async Task<ApiResponse<UpdateProfileResponse>> GetProfileAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return ApiResponse<UpdateProfileResponse>.ErrorResponse(
                        new List<string> { "User ID is required." },
                        "Get Profile Failed"
                    );
                }

                var user = await _authRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<UpdateProfileResponse>.ErrorResponse(
                        new List<string> { "User not found." },
                        "Get Profile Failed"
                    );
                }

                var response = new UpdateProfileResponse
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email ?? string.Empty,
                    PhoneNumber = user.PhoneNumber ?? string.Empty,
                    DateOfBirth = user.DateOfBirth,
                    AvatarUrl = user.AvatarUrl,
                    UpdatedAt = user.UpdatedAt
                };

                return ApiResponse<UpdateProfileResponse>.SuccessResponse(response, "Profile retrieved successfully");
            }
            catch (Exception)
            {
                return ApiResponse<UpdateProfileResponse>.ErrorResponse(
                    new List<string> { "An error occurred while retrieving profile. Please try again." },
                    "Get Profile Failed"
                );
            }
        }

        public async Task<ApiResponse<UpdateProfileResponse>> UpdateProfileAsync(string userId, UpdateProfileRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return ApiResponse<UpdateProfileResponse>.ErrorResponse(
                        new List<string> { "User ID is required." },
                        "Update Profile Failed"
                    );
                }

                var user = await _authRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<UpdateProfileResponse>.ErrorResponse(
                        new List<string> { "User not found." },
                        "Update Profile Failed"
                    );
                }
                if (!string.IsNullOrWhiteSpace(request.FirstName))
                {
                    user.FirstName = request.FirstName;
                }

                if (!string.IsNullOrWhiteSpace(request.LastName))
                {
                    user.LastName = request.LastName;
                }

                if (!string.IsNullOrWhiteSpace(request.Email))
                {
                    var existingUser = await _authRepository.FindByEmailAsync(request.Email);
                    if (existingUser != null && existingUser.Id != userId)
                    {
                        return ApiResponse<UpdateProfileResponse>.ErrorResponse(
                            new List<string> { "Email is already taken by another user." },
                            "Update Profile Failed"
                        );
                    }
                    user.Email = request.Email;
                    user.UserName = request.Email; 
                }

                if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                {
                    user.PhoneNumber = request.PhoneNumber;
                }

                if (request.DateOfBirth.HasValue)
                {
                    user.DateOfBirth = request.DateOfBirth;
                }

                if (request.AvatarFile != null)
                {
                    try
                    {
                        var avatarUrl = await _fileUploadService.UploadFileAsync(request.AvatarFile);
                        if (!string.IsNullOrEmpty(avatarUrl))
                        {
                            user.AvatarUrl = avatarUrl;
                        }
                    }
                    catch (Exception)
                    {
                        return ApiResponse<UpdateProfileResponse>.ErrorResponse(
                            new List<string> { "Failed to upload avatar image. Please try again." },
                            "Update Profile Failed"
                        );
                    }
                }

                user.UpdatedAt = DateTime.UtcNow;

                var result = await _authRepository.UpdateUserAsync(user);
                if (!result.Succeeded)
                {
                    return ApiResponse<UpdateProfileResponse>.ErrorResponse(
                        result.Errors?.Select(e => e.Description).ToList() ?? new List<string> { "Failed to update profile." },
                        "Update Profile Failed"
                    );
                }

                var response = new UpdateProfileResponse
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email ?? string.Empty,
                    PhoneNumber = user.PhoneNumber ?? string.Empty,
                    DateOfBirth = user.DateOfBirth,
                    AvatarUrl = user.AvatarUrl,
                    UpdatedAt = user.UpdatedAt
                };

                return ApiResponse<UpdateProfileResponse>.SuccessResponse(response, "Profile updated successfully");
            }
            catch (Exception)
            {
                return ApiResponse<UpdateProfileResponse>.ErrorResponse(
                    new List<string> { "An error occurred while updating profile. Please try again." },
                    "Update Profile Failed"
                );
            }
        }
    }
}
