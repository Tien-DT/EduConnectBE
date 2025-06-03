using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using EduConnect.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduConnect.Application.Services.Interface
{
    public interface IAuthService
    {
        Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest login);
        Task<ApiResponse<RegisterResponse>> RegisterAsync(RegisterRequest register);
        Task<GetNewTokenResponse> GetNewToken(GetNewTokenRequest request);
    }
}
