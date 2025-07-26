using EduConnect.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduConnect.Infrastruture.Repositories.Interface
{
    public interface IPromptRepository
    {
        // Standard methods with paging, search, sort
        Task<List<Prompt>> GetPromptsAsync(
            int pageIndex = 0,
            int pageSize = 10,
            string? searchTerm = null,
            string sortBy = "CreatedAt",
            bool ascending = false,
            string? type = null,
            string? status = null);

        Task<int> GetPromptsCountAsync(
            string? searchTerm = null,
            string? type = null,
            string? status = null);

        Task<List<Prompt>> GetPromptsByAccountIdAsync(
            string accountId,
            int pageIndex = 0,
            int pageSize = 10,
            string? searchTerm = null,
            string sortBy = "CreatedAt",
            bool ascending = false,
            string? type = null,
            string? status = null);

        Task<int> GetPromptsCountByAccountIdAsync(
            string accountId,
            string? searchTerm = null,
            string? type = null,
            string? status = null);

        // Basic CRUD operations
        Task<Prompt> CreatePromptAsync(Prompt prompt);
        Task<Prompt?> GetPromptByIdAsync(int promptId);
        Task<Prompt> UpdatePromptAsync(Prompt prompt);
        Task<bool> DeletePromptAsync(int promptId);
        Task<bool> SoftDeletePromptAsync(int promptId);

        // Legacy methods for backward compatibility
        Task<List<Prompt>> GetPromptsByAccountIdAsync(string accountId);
        Task<List<Prompt>> GetPromptsByTypeAsync(string type);
        Task<List<Prompt>> GetPromptsByStatusAsync(string status);
        Task<List<Prompt>> GetAllPromptsAsync();
        Task<List<Prompt>> GetPromptsByAccountIdAndTypeAsync(string accountId, string type);
        Task<List<Prompt>> GetPromptsByAccountIdAndStatusAsync(string accountId, string status);
        Task<int> GetTotalPromptsCountAsync();
        Task<int> GetPromptsCountByAccountIdAsync(string accountId);
        Task<List<Prompt>> GetPagedPromptsAsync(int pageIndex, int pageSize);
        Task<List<Prompt>> GetPagedPromptsByAccountIdAsync(string accountId, int pageIndex, int pageSize);
    }
}
