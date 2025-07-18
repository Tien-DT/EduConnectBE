using EduConnect.Data;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Repositories.Interface;
using EduConnect.Infrastruture.Repositories.Class;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduConnect.Infrastruture.Repositories.Class
{
    public class FeedbackRepository : BaseRepository<Feedback, int>, IFeedbackRepository
    {
        private readonly EduConnectDbContext _context;

        public FeedbackRepository(EduConnectDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<(List<Feedback> items, int totalCount)> GetFeedbacksAsync(
            int pageIndex = 0,
            int pageSize = 10,
            string? searchTerm = null,
            string? sortBy = "DateTime",
            bool ascending = false,
            string? targetFilter = null)
        {
            var query = _context.Feedbacks
                .Include(f => f.Parent)
                    .ThenInclude(p => p.Account)
                .Where(f => f.IsDeleted != true);

            // Apply search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(f => 
                    (f.Content != null && f.Content.Contains(searchTerm)) ||
                    (f.Parent.Account.FirstName + " " + f.Parent.Account.LastName).Contains(searchTerm) ||
                    (f.Target != null && f.Target.Contains(searchTerm)));
            }

            // Apply target filter
            if (!string.IsNullOrEmpty(targetFilter))
            {
                query = query.Where(f => f.Target == targetFilter);
            }

            // Apply sorting
            query = sortBy?.ToLower() switch
            {
                "content" => ascending ? query.OrderBy(f => f.Content) : query.OrderByDescending(f => f.Content),
                "parentname" => ascending ? query.OrderBy(f => f.Parent.Account.FirstName + " " + f.Parent.Account.LastName) : query.OrderByDescending(f => f.Parent.Account.FirstName + " " + f.Parent.Account.LastName),
                "target" => ascending ? query.OrderBy(f => f.Target) : query.OrderByDescending(f => f.Target),
                _ => ascending ? query.OrderBy(f => f.DateTime) : query.OrderByDescending(f => f.DateTime)
            };

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<List<Feedback>> GetFeedbacksByParentIdAsync(string parentId)
        {
            return await _context.Feedbacks
                .Include(f => f.Parent)
                    .ThenInclude(p => p.Account)
                .Where(f => f.ParentID == parentId && f.IsDeleted != true)
                .OrderByDescending(f => f.DateTime)
                .ToListAsync();
        }

        public async Task<List<Feedback>> GetFeedbacksByTargetAsync(string target)
        {
            return await _context.Feedbacks
                .Include(f => f.Parent)
                    .ThenInclude(p => p.Account)
                .Where(f => f.Target == target && f.IsDeleted != true)
                .OrderByDescending(f => f.DateTime)
                .ToListAsync();
        }
    }
}
