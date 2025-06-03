using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using EduConnect.Application.Services.Interface;
using EduConnect.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EduConnect.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest register)
        {
            var result = await authService.RegisterAsync(register);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest login)
        {
            var token = await authService.LoginAsync(login);

            if (token is null)
            {
                return BadRequest("Invalid Email or Password");
            }

            return Ok(token);
        }

        [HttpGet("checkauthor")]
        [Authorize]
        public async Task<string> CheckAuthor()
        {
            return "Authorized";
        }

        [HttpPost("GetNewToken")]
        public async Task<ActionResult<ApiResponse<GetNewTokenResponse>>> GetNewToken(GetNewTokenRequest request)
        {
            var result = await authService.GetNewToken(request);
            if (result is null)
            {
                return BadRequest(ApiResponse<GetNewTokenResponse>.ErrorResponse(
                    new List<string> { "Refresh token is invalid or has expired." },
                    "Token Refresh Failed"
                ));
            }
            return Ok(ApiResponse<GetNewTokenResponse>.SuccessResponse(result, "Token Refresh Succeeded"));
        }
    }
}

    