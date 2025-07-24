using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using EduConnect.Application.QueryHelpers;
using EduConnect.Application.Services.Interface;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduConnect.Application.Services.Class
{
    public class PromptService : IPromptService
    {
        private readonly IPromptRepository _promptRepository;

        public PromptService(IPromptRepository promptRepository)
        {
            _promptRepository = promptRepository;
        }

        // Standard methods with paging, search, sort
        public async Task<PagedApiResponse<List<PromptResponse>>> GetPromptsAsync(
            int pageIndex = 0,
            int pageSize = 10,
            string? searchTerm = null,
            string sortBy = "CreatedAt",
            bool ascending = false,
            string? type = null,
            string? status = null)
        {
            try
            {
                if (pageSize <= 0) pageSize = 10;
                if (pageIndex < 0) pageIndex = 0;

                var totalCount = await _promptRepository.GetPromptsCountAsync(searchTerm, type, status);
                var prompts = await _promptRepository.GetPromptsAsync(pageIndex, pageSize, searchTerm, sortBy, ascending, type, status);
                var promptResponses = prompts.Select(MapToPromptResponse).ToList();

                return PagedApiResponse<List<PromptResponse>>.SuccessPagedResponse(
                    promptResponses,
                    "Prompts retrieved successfully",
                    totalCount,
                    pageIndex,
                    pageSize);
            }
            catch (Exception ex)
            {
                return PagedApiResponse<List<PromptResponse>>.ErrorPagedResponse(
                    new List<string> { ex.Message },
                    "Error retrieving prompts",
                    pageIndex,
                    pageSize);
            }
        }

        public async Task<PagedApiResponse<List<PromptResponse>>> GetPromptsByAccountIdAsync(
            string accountId,
            int pageIndex = 0,
            int pageSize = 10,
            string? searchTerm = null,
            string sortBy = "CreatedAt",
            bool ascending = false,
            string? type = null,
            string? status = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(accountId))
                {
                    return PagedApiResponse<List<PromptResponse>>.ErrorPagedResponse(
                        new List<string> { "Account ID is required" },
                        "Invalid account ID",
                        pageIndex,
                        pageSize);
                }

                if (pageSize <= 0) pageSize = 10;
                if (pageIndex < 0) pageIndex = 0;

                var totalCount = await _promptRepository.GetPromptsCountByAccountIdAsync(accountId, searchTerm, type, status);
                var prompts = await _promptRepository.GetPromptsByAccountIdAsync(accountId, pageIndex, pageSize, searchTerm, sortBy, ascending, type, status);
                var promptResponses = prompts.Select(MapToPromptResponse).ToList();

                return PagedApiResponse<List<PromptResponse>>.SuccessPagedResponse(
                    promptResponses,
                    "Prompts retrieved successfully",
                    totalCount,
                    pageIndex,
                    pageSize);
            }
            catch (Exception ex)
            {
                return PagedApiResponse<List<PromptResponse>>.ErrorPagedResponse(
                    new List<string> { ex.Message },
                    "Error retrieving prompts",
                    pageIndex,
                    pageSize);
            }
        }

        public async Task<ApiResponse<PromptResponse>> CreatePromptAsync(string accountId, CreatePromptRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(accountId))
                {
                    return ApiResponse<PromptResponse>.ErrorResponse(
                        new List<string> { "Account ID is required" },
                        "Invalid account ID");
                }

                var prompt = new Prompt
                {
                    AccountId = accountId,
                    Title = request.Title,
                    PromptText = request.PromptText,
                    Type = request.Type,
                    Status = request.Status,
                    CreatedAt = DateTime.UtcNow
                };

                var createdPrompt = await _promptRepository.CreatePromptAsync(prompt);
                var promptResponse = MapToPromptResponse(createdPrompt);

                return ApiResponse<PromptResponse>.SuccessResponse(
                    promptResponse,
                    "Prompt created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<PromptResponse>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Error creating prompt");
            }
        }

        public async Task<ApiResponse<PromptResponse>> GetPromptByIdAsync(int promptId)
        {
            try
            {
                var prompt = await _promptRepository.GetPromptByIdAsync(promptId);
                
                if (prompt == null)
                {
                    return ApiResponse<PromptResponse>.ErrorResponse(
                        new List<string> { "Prompt not found" },
                        "Prompt not found");
                }

                var promptResponse = MapToPromptResponse(prompt);

                return ApiResponse<PromptResponse>.SuccessResponse(
                    promptResponse,
                    "Prompt retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<PromptResponse>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Error retrieving prompt");
            }
        }

        public async Task<ApiResponse<List<PromptResponse>>> GetPromptsByAccountIdAsync(string accountId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(accountId))
                {
                    return ApiResponse<List<PromptResponse>>.ErrorResponse(
                        new List<string> { "Account ID is required" },
                        "Invalid account ID");
                }

                var prompts = await _promptRepository.GetPromptsByAccountIdAsync(accountId);
                var promptResponses = prompts.Select(MapToPromptResponse).ToList();

                return ApiResponse<List<PromptResponse>>.SuccessResponse(
                    promptResponses,
                    $"Retrieved {promptResponses.Count} prompts for account");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<PromptResponse>>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Error retrieving prompts");
            }
        }

        public async Task<ApiResponse<List<PromptResponse>>> GetPromptsByTypeAsync(string type)
        {
            try
            {
                var prompts = await _promptRepository.GetPromptsByTypeAsync(type);
                var promptResponses = prompts.Select(MapToPromptResponse).ToList();

                return ApiResponse<List<PromptResponse>>.SuccessResponse(
                    promptResponses,
                    $"Retrieved {promptResponses.Count} prompts of type: {type}");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<PromptResponse>>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Error retrieving prompts by type");
            }
        }

        public async Task<ApiResponse<List<PromptResponse>>> GetPromptsByStatusAsync(string status)
        {
            try
            {
                var prompts = await _promptRepository.GetPromptsByStatusAsync(status);
                var promptResponses = prompts.Select(MapToPromptResponse).ToList();

                return ApiResponse<List<PromptResponse>>.SuccessResponse(
                    promptResponses,
                    $"Retrieved {promptResponses.Count} prompts with status: {status}");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<PromptResponse>>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Error retrieving prompts by status");
            }
        }

        public async Task<PagedApiResponse<List<PromptResponse>>> GetAllPromptsAsync(int pageIndex = 0, int pageSize = 10)
        {
            try
            {
                if (pageSize <= 0) pageSize = 10;
                if (pageIndex < 0) pageIndex = 0;

                var totalCount = await _promptRepository.GetTotalPromptsCountAsync();
                var prompts = await _promptRepository.GetPagedPromptsAsync(pageIndex, pageSize);
                var promptResponses = prompts.Select(MapToPromptResponse).ToList();

                return PagedApiResponse<List<PromptResponse>>.SuccessPagedResponse(
                    promptResponses,
                    "Prompts retrieved successfully",
                    totalCount,
                    pageIndex,
                    pageSize);
            }
            catch (Exception ex)
            {
                return PagedApiResponse<List<PromptResponse>>.ErrorPagedResponse(
                    new List<string> { ex.Message },
                    "Error retrieving prompts",
                    pageIndex,
                    pageSize);
            }
        }

        public async Task<ApiResponse<PromptResponse>> UpdatePromptAsync(string accountId, UpdatePromptRequest request)
        {
            try
            {
                var prompt = await _promptRepository.GetPromptByIdAsync(request.PromptId);
                
                if (prompt == null)
                {
                    return ApiResponse<PromptResponse>.ErrorResponse(
                        new List<string> { "Prompt not found" },
                        "Prompt not found");
                }

                // Check if the user owns this prompt
                if (prompt.AccountId != accountId)
                {
                    return ApiResponse<PromptResponse>.ErrorResponse(
                        new List<string> { "Unauthorized to update this prompt" },
                        "Unauthorized");
                }

                // Update fields if provided
                if (!string.IsNullOrWhiteSpace(request.Title))
                    prompt.Title = request.Title;
                
                if (!string.IsNullOrWhiteSpace(request.PromptText))
                    prompt.PromptText = request.PromptText;
                
                if (!string.IsNullOrWhiteSpace(request.Type))
                    prompt.Type = request.Type;
                
                if (!string.IsNullOrWhiteSpace(request.Status))
                    prompt.Status = request.Status;

                var updatedPrompt = await _promptRepository.UpdatePromptAsync(prompt);
                var promptResponse = MapToPromptResponse(updatedPrompt);

                return ApiResponse<PromptResponse>.SuccessResponse(
                    promptResponse,
                    "Prompt updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<PromptResponse>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Error updating prompt");
            }
        }

        public async Task<ApiResponse<bool>> DeletePromptAsync(int promptId, string accountId)
        {
            try
            {
                var prompt = await _promptRepository.GetPromptByIdAsync(promptId);
                
                if (prompt == null)
                {
                    return ApiResponse<bool>.ErrorResponse(
                        new List<string> { "Prompt not found" },
                        "Prompt not found");
                }

                // Check if the user owns this prompt
                if (prompt.AccountId != accountId)
                {
                    return ApiResponse<bool>.ErrorResponse(
                        new List<string> { "Unauthorized to delete this prompt" },
                        "Unauthorized");
                }

                var result = await _promptRepository.DeletePromptAsync(promptId);

                return ApiResponse<bool>.SuccessResponse(
                    result,
                    result ? "Prompt deleted successfully" : "Failed to delete prompt");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Error deleting prompt");
            }
        }

        public async Task<ApiResponse<bool>> SoftDeletePromptAsync(int promptId, string accountId)
        {
            try
            {
                var prompt = await _promptRepository.GetPromptByIdAsync(promptId);
                
                if (prompt == null)
                {
                    return ApiResponse<bool>.ErrorResponse(
                        new List<string> { "Prompt not found" },
                        "Prompt not found");
                }

                // Check if the user owns this prompt
                if (prompt.AccountId != accountId)
                {
                    return ApiResponse<bool>.ErrorResponse(
                        new List<string> { "Unauthorized to delete this prompt" },
                        "Unauthorized");
                }

                var result = await _promptRepository.SoftDeletePromptAsync(promptId);

                return ApiResponse<bool>.SuccessResponse(
                    result,
                    result ? "Prompt soft deleted successfully" : "Failed to soft delete prompt");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Error soft deleting prompt");
            }
        }

        public async Task<PagedApiResponse<List<PromptResponse>>> GetPromptsByAccountIdPaginatedAsync(string accountId, int pageIndex = 0, int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(accountId))
                {
                    return PagedApiResponse<List<PromptResponse>>.ErrorPagedResponse(
                        new List<string> { "Account ID is required" },
                        "Invalid account ID",
                        pageIndex,
                        pageSize);
                }

                if (pageSize <= 0) pageSize = 10;
                if (pageIndex < 0) pageIndex = 0;

                var totalCount = await _promptRepository.GetPromptsCountByAccountIdAsync(accountId);
                var prompts = await _promptRepository.GetPagedPromptsByAccountIdAsync(accountId, pageIndex, pageSize);
                var promptResponses = prompts.Select(MapToPromptResponse).ToList();

                return PagedApiResponse<List<PromptResponse>>.SuccessPagedResponse(
                    promptResponses,
                    "Prompts retrieved successfully",
                    totalCount,
                    pageIndex,
                    pageSize);
            }
            catch (Exception ex)
            {
                return PagedApiResponse<List<PromptResponse>>.ErrorPagedResponse(
                    new List<string> { ex.Message },
                    "Error retrieving prompts",
                    pageIndex,
                    pageSize);
            }
        }

        public async Task<ApiResponse<List<PromptResponse>>> GetPromptsByAccountIdAndTypeAsync(string accountId, string type)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(accountId))
                {
                    return ApiResponse<List<PromptResponse>>.ErrorResponse(
                        new List<string> { "Account ID is required" },
                        "Invalid account ID");
                }

                var prompts = await _promptRepository.GetPromptsByAccountIdAndTypeAsync(accountId, type);
                var promptResponses = prompts.Select(MapToPromptResponse).ToList();

                return ApiResponse<List<PromptResponse>>.SuccessResponse(
                    promptResponses,
                    $"Retrieved {promptResponses.Count} prompts for account with type: {type}");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<PromptResponse>>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Error retrieving prompts");
            }
        }

        public async Task<ApiResponse<List<PromptResponse>>> GetPromptsByAccountIdAndStatusAsync(string accountId, string status)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(accountId))
                {
                    return ApiResponse<List<PromptResponse>>.ErrorResponse(
                        new List<string> { "Account ID is required" },
                        "Invalid account ID");
                }

                var prompts = await _promptRepository.GetPromptsByAccountIdAndStatusAsync(accountId, status);
                var promptResponses = prompts.Select(MapToPromptResponse).ToList();

                return ApiResponse<List<PromptResponse>>.SuccessResponse(
                    promptResponses,
                    $"Retrieved {promptResponses.Count} prompts for account with status: {status}");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<PromptResponse>>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Error retrieving prompts");
            }
        }

        private PromptResponse MapToPromptResponse(Prompt prompt)
        {
            return new PromptResponse
            {
                PromptId = prompt.PromptId,
                AccountId = prompt.AccountId,
                Title = prompt.Title,
                PromptText = prompt.PromptText,
                Type = prompt.Type,
                Status = prompt.Status,
                IsDeleted = prompt.IsDeleted,
                CreatedAt = prompt.CreatedAt,
                UpdatedAt = prompt.UpdatedAt,
                CreatedByName = $"{prompt.Account?.FirstName} {prompt.Account?.LastName}".Trim(),
                CreatedByEmail = prompt.Account?.Email ?? string.Empty
            };
        }
    }
}
