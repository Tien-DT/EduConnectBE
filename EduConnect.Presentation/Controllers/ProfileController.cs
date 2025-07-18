using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using EduConnect.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EduConnect.Presentation.Controllers
{
    [Route("api/v1/profiles")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<ApiResponse<UpdateProfileResponse>>> GetProfile(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest(new ApiResponse<UpdateProfileResponse>
                {
                    Success = false,
                    Message = "User ID is required",
                    Error = new List<string> { "User ID parameter is missing or empty." }
                });
            }

            var result = await _profileService.GetProfileAsync(userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPut("{userId}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResponse<UpdateProfileResponse>>> UpdateProfileById(string userId, [FromForm] UpdateProfileRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest(new ApiResponse<UpdateProfileResponse>
                {
                    Success = false,
                    Message = "User ID is required",
                    Error = new List<string> { "User ID parameter is missing or empty." }
                });
            }

            var result = await _profileService.UpdateProfileAsync(userId, request);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
