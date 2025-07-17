using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using EduConnect.Application.Services.Interface;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduConnect.Application.Services.Class
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _feedbackRepository;
        private readonly IParentRepository _parentRepository;

        public FeedbackService(IFeedbackRepository feedbackRepository, IParentRepository parentRepository)
        {
            _feedbackRepository = feedbackRepository;
            _parentRepository = parentRepository;
        }

        public async Task<ApiResponse<FeedbackResponse>> CreateFeedbackAsync(CreateFeedbackRequest request)
        {
            try
            {
                // Validate parent exists
                var parent = await _parentRepository.GetByIdAsync(request.ParentID);
                if (parent == null)
                {
                    return ApiResponse<FeedbackResponse>.ErrorResponse(
                        new List<string> { "Parent not found" },
                        "Invalid parent ID");
                }

                var feedback = new Feedback
                {
                    ParentID = request.ParentID,
                    Content = request.Content,
                    Target = request.Target,
                    DateTime = DateTime.UtcNow,
                    IsDeleted = false
                };

                var createdFeedback = await _feedbackRepository.CreateAsync(feedback);
                
                // Get feedback with parent details
                var feedbackWithParent = await _feedbackRepository.GetByIdAsync(createdFeedback.FeedbackID);
                
                var response = MapToFeedbackResponse(feedbackWithParent);

                return ApiResponse<FeedbackResponse>.SuccessResponse(
                    response,
                    "Feedback created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<FeedbackResponse>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Error creating feedback");
            }
        }

        public async Task<ApiResponse<FeedbackResponse>> UpdateFeedbackAsync(int feedbackId, UpdateFeedbackRequest request)
        {
            try
            {
                var existingFeedback = await _feedbackRepository.GetByIdAsync(feedbackId);
                if (existingFeedback == null || existingFeedback.IsDeleted == true)
                {
                    return ApiResponse<FeedbackResponse>.ErrorResponse(
                        new List<string> { "Feedback not found" },
                        "Feedback not found");
                }

                existingFeedback.Content = request.Content;
                existingFeedback.Target = request.Target;

                var updatedFeedback = await _feedbackRepository.UpdateAsync(feedbackId, existingFeedback);
                if (updatedFeedback == null)
                {
                    return ApiResponse<FeedbackResponse>.ErrorResponse(
                        new List<string> { "Failed to update feedback" },
                        "Update failed");
                }

                var response = MapToFeedbackResponse(updatedFeedback);

                return ApiResponse<FeedbackResponse>.SuccessResponse(
                    response,
                    "Feedback updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<FeedbackResponse>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Error updating feedback");
            }
        }

        public async Task<ApiResponse<bool>> DeleteFeedbackAsync(int feedbackId)
        {
            try
            {
                var feedback = await _feedbackRepository.GetByIdAsync(feedbackId);
                if (feedback == null || feedback.IsDeleted == true)
                {
                    return ApiResponse<bool>.ErrorResponse(
                        new List<string> { "Feedback not found" },
                        "Feedback not found");
                }

                feedback.IsDeleted = true;
                var result = await _feedbackRepository.UpdateAsync(feedbackId, feedback);

                if (result != null)
                {
                    return ApiResponse<bool>.SuccessResponse(true, "Feedback deleted successfully");
                }

                return ApiResponse<bool>.ErrorResponse(
                    new List<string> { "Failed to delete feedback" },
                    "Delete failed");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Error deleting feedback");
            }
        }

        public async Task<ApiResponse<FeedbackResponse>> GetFeedbackByIdAsync(int feedbackId)
        {
            try
            {
                var feedback = await _feedbackRepository.GetByIdAsync(feedbackId);
                if (feedback == null || feedback.IsDeleted == true)
                {
                    return ApiResponse<FeedbackResponse>.ErrorResponse(
                        new List<string> { "Feedback not found" },
                        "Feedback not found");
                }

                var response = MapToFeedbackResponse(feedback);

                return ApiResponse<FeedbackResponse>.SuccessResponse(
                    response,
                    "Feedback retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<FeedbackResponse>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Error retrieving feedback");
            }
        }

        public async Task<PagedApiResponse<List<FeedbackResponse>>> GetFeedbacksAsync(
            int pageIndex = 0,
            int pageSize = 10,
            string? searchTerm = null,
            string sortBy = "DateTime",
            bool ascending = false,
            string? targetFilter = null)
        {
            try
            {
                var (items, totalCount) = await _feedbackRepository.GetFeedbacksAsync(
                    pageIndex, pageSize, searchTerm, sortBy, ascending, targetFilter);

                var responses = items.Select(MapToFeedbackResponse).ToList();

                return PagedApiResponse<List<FeedbackResponse>>.SuccessPagedResponse(
                    responses,
                    "Feedbacks retrieved successfully",
                    totalCount,
                    pageIndex,
                    pageSize);
            }
            catch (Exception ex)
            {
                return PagedApiResponse<List<FeedbackResponse>>.ErrorPagedResponse(
                    new List<string> { ex.Message },
                    "Error retrieving feedbacks",
                    pageIndex,
                    pageSize);
            }
        }

        public async Task<ApiResponse<List<FeedbackResponse>>> GetFeedbacksByParentIdAsync(string parentId)
        {
            try
            {
                var feedbacks = await _feedbackRepository.GetFeedbacksByParentIdAsync(parentId);
                var responses = feedbacks.Select(MapToFeedbackResponse).ToList();

                return ApiResponse<List<FeedbackResponse>>.SuccessResponse(
                    responses,
                    "Feedbacks retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<FeedbackResponse>>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Error retrieving feedbacks");
            }
        }

        public async Task<ApiResponse<List<FeedbackResponse>>> GetFeedbacksByTargetAsync(string target)
        {
            try
            {
                var feedbacks = await _feedbackRepository.GetFeedbacksByTargetAsync(target);
                var responses = feedbacks.Select(MapToFeedbackResponse).ToList();

                return ApiResponse<List<FeedbackResponse>>.SuccessResponse(
                    responses,
                    "Feedbacks retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<FeedbackResponse>>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Error retrieving feedbacks");
            }
        }

        private FeedbackResponse MapToFeedbackResponse(Feedback feedback)
        {
            return new FeedbackResponse
            {
                FeedbackId = feedback.FeedbackID,
                ParentId = feedback.ParentID,
                ParentName = feedback.Parent != null && feedback.Parent.Account != null 
                    ? $"{feedback.Parent.Account.FirstName} {feedback.Parent.Account.LastName}".Trim()
                    : "Unknown",
                Content = feedback.Content ?? string.Empty,
                DateTime = feedback.DateTime,
                Target = feedback.Target ?? string.Empty,
                IsDeleted = feedback.IsDeleted ?? false
            };
        }
    }
}
