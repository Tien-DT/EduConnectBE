using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;

namespace EduConnect.Application.Services.Interface
{
    public interface IRoleService
    {
        Task<ApiResponse<AssignRoleResponse>> AssignStudentRoleAsync(AssignStudentRoleListRequest request);
        Task<ApiResponse<AssignRoleResponse>> AssignTeacherRoleAsync(AssignTeacherRoleListRequest request);
        Task<ApiResponse<AssignRoleResponse>> AssignParentRoleAsync(AssignParentRoleListRequest request);
    }
}