using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EduConnectBE.Models;
using EduConnectBE.Services;
using EduConnect.Application.DTOs.Response;

namespace EduConnectBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<TokenResponse>> Login([FromBody] LoginRequest request)
        {
            var (tokenResponse, serviceResponse) = await _authService.LoginAsync(request);
            
            if (serviceResponse != null)
            {
                return BadRequest(new { message = serviceResponse });
            }
            
            return Ok(tokenResponse);
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
        {
            var response = await _authService.RegisterAsync(request);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var response = await _authService.RefreshTokenAsync(request.RefreshToken);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
} 