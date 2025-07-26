using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using EduConnect.Application.QueryHelpers;
using System.Threading.Tasks;

namespace EduConnect.Application.Services.Interface
{
    public interface IPromptService
    {
        // Standard methods with paging, search, sort
        Task<PagedApiResponse<List<PromptResponse>>> GetPromptsAsync(
            int pageIndex = 0, 
            int pageSize = 10, 
            string? searchTerm = null, 
            string sortBy = "CreatedAt", 
            bool ascending = false,
            string? type = null,
            string? status = null);

        Task<PagedApiResponse<List<PromptResponse>>> GetPromptsByAccountIdAsync(
            string accountId,
            int pageIndex = 0,
            int pageSize = 10,
            string? searchTerm = null,
            string sortBy = "CreatedAt",
            bool ascending = false,
            string? type = null,
            string? status = null);

        Task<ApiResponse<PromptResponse>> CreatePromptAsync(string accountId, CreatePromptRequest request);
        Task<ApiResponse<PromptResponse>> GetPromptByIdAsync(int promptId);
        Task<ApiResponse<PromptResponse>> UpdatePromptAsync(string accountId, UpdatePromptRequest request);
        Task<ApiResponse<bool>> DeletePromptAsync(int promptId, string accountId);
        Task<ApiResponse<bool>> SoftDeletePromptAsync(int promptId, string accountId);

        // Legacy methods for backward compatibility
        Task<ApiResponse<List<PromptResponse>>> GetPromptsByAccountIdAsync(string accountId);
        Task<ApiResponse<List<PromptResponse>>> GetPromptsByTypeAsync(string type);
        Task<ApiResponse<List<PromptResponse>>> GetPromptsByStatusAsync(string status);
        Task<PagedApiResponse<List<PromptResponse>>> GetAllPromptsAsync(int pageIndex = 0, int pageSize = 10);
        Task<PagedApiResponse<List<PromptResponse>>> GetPromptsByAccountIdPaginatedAsync(string accountId, int pageIndex = 0, int pageSize = 10);
        Task<ApiResponse<List<PromptResponse>>> GetPromptsByAccountIdAndTypeAsync(string accountId, string type);
        Task<ApiResponse<List<PromptResponse>>> GetPromptsByAccountIdAndStatusAsync(string accountId, string status);
    }
}
