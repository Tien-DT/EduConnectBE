using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using System.Threading.Tasks;

namespace EduConnect.Application.Services.Interface
{
    public interface IProfileService
    {
        Task<ApiResponse<UpdateProfileResponse>> UpdateProfileAsync(string userId, UpdateProfileRequest request);
        Task<ApiResponse<UpdateProfileResponse>> GetProfileAsync(string userId);
    }
}
