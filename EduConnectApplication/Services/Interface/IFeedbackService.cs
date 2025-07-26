using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduConnect.Application.Services.Interface
{
    public interface IFeedbackService
    {
        Task<ApiResponse<FeedbackResponse>> CreateFeedbackAsync(CreateFeedbackRequest request);
        Task<ApiResponse<FeedbackResponse>> UpdateFeedbackAsync(int feedbackId, UpdateFeedbackRequest request);
        Task<ApiResponse<bool>> DeleteFeedbackAsync(int feedbackId);
        Task<ApiResponse<FeedbackResponse>> GetFeedbackByIdAsync(int feedbackId);
        Task<PagedApiResponse<List<FeedbackResponse>>> GetFeedbacksAsync(
            int pageIndex = 0,
            int pageSize = 10,
            string? searchTerm = null,
            string sortBy = "DateTime",
            bool ascending = false,
            string? targetFilter = null);
        Task<PagedApiResponse<List<FeedbackResponse>>> GetFeedbacksByParentIdAsync(string parentId, int pageIndex = 0, int pageSize = 10);
        Task<ApiResponse<List<FeedbackResponse>>> GetFeedbacksByTargetAsync(string target);
    }
}
