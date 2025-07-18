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
    [Route("api/v1/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest register)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RegisterAsync(register);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest login)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.LoginAsync(login);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("token/refresh")]
        public async Task<ActionResult<ApiResponse<GetNewTokenResponse>>> GetNewToken(GetNewTokenRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.GetNewTokenAsync(request);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        //[HttpPost("google-login")]
        //public async Task<ActionResult<ApiResponse<LoginResponse>>> GoogleLogin(GoogleLoginRequest request)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    var result = await _authService.GoogleLoginAsync(request);
        //    if (!result.Success)
        //    {
        //        return BadRequest(result);
        //    }
        //    return Ok(result);
        //}

        [HttpPost("login/google")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> GoogleTokenLogin(GoogleTokenLoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.GoogleTokenLoginAsync(request);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("user/role")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserRoleIdResponse>>> GetUserRoleId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(ApiResponse<UserRoleIdResponse>.ErrorResponse(
                    new List<string> { "Invalid authentication token" },
                    "User ID not found in token"
                ));
            }

            var result = await _authService.GetUserRoleIdAsync(userId);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }
            
            return Ok(result);
        }

        [HttpGet("email/verify")]
        public async Task<ActionResult<ApiResponse<string>>> VerifyEmail([FromQuery] string token, [FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(
                    new List<string> { "Token and email are required" },
                    "Invalid request"
                ));
            }

            var request = new EmailVerificationRequest
            {
                Token = token,
                Email = email
            };

            var result = await _authService.VerifyEmailAsync(request);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }
            
            return Ok(result);
        }
    }
}