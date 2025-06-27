using EduConnect.Data;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduConnect.Infrastruture.Repositories.Class
{
    public class ParentRepository : IParentRepository
    {
        private readonly EduConnectDbContext _context;

        public ParentRepository(EduConnectDbContext context)
        {
            _context = context;
        }

        public async Task<Parent> CreateAsync(Parent entity)
        {
            await _context.Parents.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var parent = await _context.Parents.FindAsync(id);
            if (parent == null)
                return false;

            parent.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Parent>> GetAllAsync()
        {
            return await _context.Parents
                .Include(p => p.Account)
                .Include(p => p.Students)
                    .ThenInclude(s => s.Account)
                .Where(p => p.IsDeleted != true)
                .ToListAsync();
        }

        public async Task<IEnumerable<Parent>> GetAllActiveParentsAsync()
        {
            return await _context.Parents
                .Include(p => p.Account)
                .Include(p => p.Students)
                    .ThenInclude(s => s.Account)
                .Where(p => p.IsDeleted != true && p.Account.IsActive)
                .ToListAsync();
        }

        public async Task<Parent?> GetByIdAsync(string id)
        {
            return await _context.Parents
                .Include(p => p.Account)
                .Include(p => p.Students)
                    .ThenInclude(s => s.Account)
                .FirstOrDefaultAsync(p => p.ParentID == id && p.IsDeleted != true);
        }

        public async Task<Parent?> GetParentWithDetailsAsync(string parentId)
        {
            return await _context.Parents
                .Include(p => p.Account)
                .Include(p => p.Students)
                    .ThenInclude(s => s.Account)
                .Include(p => p.Feedbacks)
                .FirstOrDefaultAsync(p => p.ParentID == parentId && p.IsDeleted != true);
        }

        public async Task<IEnumerable<Parent>> SearchParentsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllActiveParentsAsync();

            searchTerm = searchTerm.ToLower();
            return await _context.Parents
                .Include(p => p.Account)
                .Include(p => p.Students)
                    .ThenInclude(s => s.Account)
                .Where(p => p.IsDeleted != true && p.Account.IsActive &&
                    (p.FullName != null && p.FullName.ToLower().Contains(searchTerm) ||
                     p.ParentID.ToLower().Contains(searchTerm) ||
                     p.Account.Email != null && p.Account.Email.ToLower().Contains(searchTerm) ||
                     p.Account.FirstName.ToLower().Contains(searchTerm) ||
                     p.Account.LastName.ToLower().Contains(searchTerm)))
                .ToListAsync();
        }

        public async Task<IEnumerable<Parent>> SortParentsAsync(string sortBy = "FullName", bool ascending = true)
        {
            var query = _context.Parents
                .Include(p => p.Account)
                .Include(p => p.Students)
                    .ThenInclude(s => s.Account)
                .Where(p => p.IsDeleted != true && p.Account.IsActive);

            switch (sortBy.ToLower())
            {
                case "fullname":
                    query = ascending 
                        ? query.OrderBy(p => p.FullName)
                        : query.OrderByDescending(p => p.FullName);
                    break;
                case "parentid":
                    query = ascending 
                        ? query.OrderBy(p => p.ParentID)
                        : query.OrderByDescending(p => p.ParentID);
                    break;
                case "email":
                    query = ascending 
                        ? query.OrderBy(p => p.Account.Email)
                        : query.OrderByDescending(p => p.Account.Email);
                    break;
                case "createdat":
                    query = ascending 
                        ? query.OrderBy(p => p.Account.CreatedAt)
                        : query.OrderByDescending(p => p.Account.CreatedAt);
                    break;
                default:
                    query = ascending 
                        ? query.OrderBy(p => p.FullName)
                        : query.OrderByDescending(p => p.FullName);
                    break;
            }

            return await query.ToListAsync();
        }

        public async Task<(IEnumerable<Parent> Parents, int TotalCount)> GetPagedParentsAsync(
            int pageIndex, 
            int pageSize, 
            string? searchTerm = null, 
            string sortBy = "FullName", 
            bool ascending = true)
        {
            var query = _context.Parents
                .Include(p => p.Account)
                .Include(p => p.Students)
                    .ThenInclude(s => s.Account)
                .Where(p => p.IsDeleted != true && p.Account.IsActive);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(p => 
                    (p.FullName != null && p.FullName.ToLower().Contains(searchTerm)) ||
                    p.ParentID.ToLower().Contains(searchTerm) ||
                    (p.Account.Email != null && p.Account.Email.ToLower().Contains(searchTerm)) ||
                    p.Account.FirstName.ToLower().Contains(searchTerm) ||
                    p.Account.LastName.ToLower().Contains(searchTerm));
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            switch (sortBy.ToLower())
            {
                case "fullname":
                    query = ascending 
                        ? query.OrderBy(p => p.FullName)
                        : query.OrderByDescending(p => p.FullName);
                    break;
                case "parentid":
                    query = ascending 
                        ? query.OrderBy(p => p.ParentID)
                        : query.OrderByDescending(p => p.ParentID);
                    break;
                case "email":
                    query = ascending 
                        ? query.OrderBy(p => p.Account.Email)
                        : query.OrderByDescending(p => p.Account.Email);
                    break;
                case "createdat":
                    query = ascending 
                        ? query.OrderBy(p => p.Account.CreatedAt)
                        : query.OrderByDescending(p => p.Account.CreatedAt);
                    break;
                default:
                    query = ascending 
                        ? query.OrderBy(p => p.FullName)
                        : query.OrderByDescending(p => p.FullName);
                    break;
            }

            // Apply pagination
            var parents = await query
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (parents, totalCount);
        }

        public async Task<Parent?> UpdateAsync(string id, Parent entity)
        {
            var existingParent = await _context.Parents.FindAsync(id);
            if (existingParent == null)
                return null;

            existingParent.FullName = entity.FullName;
            existingParent.IsDeleted = entity.IsDeleted;

            await _context.SaveChangesAsync();
            return existingParent;
        }

        public IQueryable<Parent> GetParentQueryable()
        {
            return _context.Parents
                .Include(p => p.Account)
                .Include(p => p.Students)
                    .ThenInclude(s => s.Account)
                .Where(p => p.IsDeleted != true && p.Account.IsActive)
                .AsQueryable();
        }
    }
}
