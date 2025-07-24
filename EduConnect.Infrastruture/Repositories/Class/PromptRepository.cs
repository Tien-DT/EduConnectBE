using EduConnect.Data;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduConnect.Infrastruture.Repositories
{
    public class PromptRepository : IPromptRepository
    {
        private readonly EduConnectDbContext _context;

        public PromptRepository(EduConnectDbContext context)
        {
            _context = context;
        }

        // Standard methods with paging, search, sort
        public async Task<List<Prompt>> GetPromptsAsync(
            int pageIndex = 0,
            int pageSize = 10,
            string? searchTerm = null,
            string sortBy = "CreatedAt",
            bool ascending = false,
            string? type = null,
            string? status = null)
        {
            var query = _context.Prompts
                .Include(p => p.Account)
                .Where(p => !p.IsDeleted);

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                query = query.Where(p => 
                    p.Title.ToLower().Contains(searchLower) ||
                    p.PromptText.ToLower().Contains(searchLower));
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(p => p.Type == type);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(p => p.Status == status);
            }

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "title" => ascending ? query.OrderBy(p => p.Title) : query.OrderByDescending(p => p.Title),
                "type" => ascending ? query.OrderBy(p => p.Type) : query.OrderByDescending(p => p.Type),
                "status" => ascending ? query.OrderBy(p => p.Status) : query.OrderByDescending(p => p.Status),
                "updatedat" => ascending ? query.OrderBy(p => p.UpdatedAt) : query.OrderByDescending(p => p.UpdatedAt),
                _ => ascending ? query.OrderBy(p => p.CreatedAt) : query.OrderByDescending(p => p.CreatedAt)
            };

            return await query
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetPromptsCountAsync(
            string? searchTerm = null,
            string? type = null,
            string? status = null)
        {
            var query = _context.Prompts.Where(p => !p.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                query = query.Where(p => 
                    p.Title.ToLower().Contains(searchLower) ||
                    p.PromptText.ToLower().Contains(searchLower));
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(p => p.Type == type);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(p => p.Status == status);
            }

            return await query.CountAsync();
        }

        public async Task<List<Prompt>> GetPromptsByAccountIdAsync(
            string accountId,
            int pageIndex = 0,
            int pageSize = 10,
            string? searchTerm = null,
            string sortBy = "CreatedAt",
            bool ascending = false,
            string? type = null,
            string? status = null)
        {
            var query = _context.Prompts
                .Include(p => p.Account)
                .Where(p => p.AccountId == accountId && !p.IsDeleted);

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                query = query.Where(p => 
                    p.Title.ToLower().Contains(searchLower) ||
                    p.PromptText.ToLower().Contains(searchLower));
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(p => p.Type == type);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(p => p.Status == status);
            }

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "title" => ascending ? query.OrderBy(p => p.Title) : query.OrderByDescending(p => p.Title),
                "type" => ascending ? query.OrderBy(p => p.Type) : query.OrderByDescending(p => p.Type),
                "status" => ascending ? query.OrderBy(p => p.Status) : query.OrderByDescending(p => p.Status),
                "updatedat" => ascending ? query.OrderBy(p => p.UpdatedAt) : query.OrderByDescending(p => p.UpdatedAt),
                _ => ascending ? query.OrderBy(p => p.CreatedAt) : query.OrderByDescending(p => p.CreatedAt)
            };

            return await query
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetPromptsCountByAccountIdAsync(
            string accountId,
            string? searchTerm = null,
            string? type = null,
            string? status = null)
        {
            var query = _context.Prompts.Where(p => p.AccountId == accountId && !p.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                query = query.Where(p => 
                    p.Title.ToLower().Contains(searchLower) ||
                    p.PromptText.ToLower().Contains(searchLower));
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(p => p.Type == type);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(p => p.Status == status);
            }

            return await query.CountAsync();
        }

        public async Task<Prompt> CreatePromptAsync(Prompt prompt)
        {
            prompt.CreatedAt = DateTime.UtcNow;
            _context.Prompts.Add(prompt);
            await _context.SaveChangesAsync();
            return prompt;
        }

        public async Task<Prompt?> GetPromptByIdAsync(int promptId)
        {
            return await _context.Prompts
                .Include(p => p.Account)
                .FirstOrDefaultAsync(p => p.PromptId == promptId && !p.IsDeleted);
        }

        public async Task<List<Prompt>> GetPromptsByAccountIdAsync(string accountId)
        {
            return await _context.Prompts
                .Include(p => p.Account)
                .Where(p => p.AccountId == accountId && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Prompt>> GetPromptsByTypeAsync(string type)
        {
            return await _context.Prompts
                .Include(p => p.Account)
                .Where(p => p.Type == type && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Prompt>> GetPromptsByStatusAsync(string status)
        {
            return await _context.Prompts
                .Include(p => p.Account)
                .Where(p => p.Status == status && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Prompt>> GetAllPromptsAsync()
        {
            return await _context.Prompts
                .Include(p => p.Account)
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Prompt> UpdatePromptAsync(Prompt prompt)
        {
            prompt.UpdatedAt = DateTime.UtcNow;
            _context.Prompts.Update(prompt);
            await _context.SaveChangesAsync();
            return prompt;
        }

        public async Task<bool> DeletePromptAsync(int promptId)
        {
            var prompt = await _context.Prompts.FindAsync(promptId);
            if (prompt == null) return false;

            _context.Prompts.Remove(prompt);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeletePromptAsync(int promptId)
        {
            var prompt = await _context.Prompts.FindAsync(promptId);
            if (prompt == null) return false;

            prompt.IsDeleted = true;
            prompt.UpdatedAt = DateTime.UtcNow;
            _context.Prompts.Update(prompt);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Prompt>> GetPromptsByAccountIdAndTypeAsync(string accountId, string type)
        {
            return await _context.Prompts
                .Include(p => p.Account)
                .Where(p => p.AccountId == accountId && p.Type == type && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Prompt>> GetPromptsByAccountIdAndStatusAsync(string accountId, string status)
        {
            return await _context.Prompts
                .Include(p => p.Account)
                .Where(p => p.AccountId == accountId && p.Status == status && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetTotalPromptsCountAsync()
        {
            return await _context.Prompts
                .CountAsync(p => !p.IsDeleted);
        }

        public async Task<int> GetPromptsCountByAccountIdAsync(string accountId)
        {
            return await _context.Prompts
                .CountAsync(p => p.AccountId == accountId && !p.IsDeleted);
        }

        public async Task<List<Prompt>> GetPagedPromptsAsync(int pageIndex, int pageSize)
        {
            return await _context.Prompts
                .Include(p => p.Account)
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<Prompt>> GetPagedPromptsByAccountIdAsync(string accountId, int pageIndex, int pageSize)
        {
            return await _context.Prompts
                .Include(p => p.Account)
                .Where(p => p.AccountId == accountId && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
