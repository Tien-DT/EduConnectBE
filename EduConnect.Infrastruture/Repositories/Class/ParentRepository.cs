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
                .Include(p => p.Students)
                    .ThenInclude(s => s.Class)
                .Where(p => p.IsDeleted != true)
                .ToListAsync();
        }

        public async Task<IEnumerable<Parent>> GetAllActiveParentsAsync()
        {
            return await _context.Parents
                .Include(p => p.Account)
                .Include(p => p.Students)
                    .ThenInclude(s => s.Account)
                .Include(p => p.Students)
                    .ThenInclude(s => s.Class)
                .Where(p => p.IsDeleted != true && p.Account.IsActive)
                .ToListAsync();
        }

        public async Task<Parent?> GetByIdAsync(string id)
        {
            return await _context.Parents
                .Include(p => p.Account)
                .Include(p => p.Students)
                    .ThenInclude(s => s.Account)
                .Include(p => p.Students)
                    .ThenInclude(s => s.Class)
                .FirstOrDefaultAsync(p => p.ParentID == id && p.IsDeleted != true);
        }

        public async Task<Parent?> GetParentWithDetailsAsync(string parentId)
        {
            var parent = await _context.Parents
                .Include(p => p.Account)
                .Include(p => p.Feedbacks)
                .FirstOrDefaultAsync(p => p.ParentID == parentId && p.IsDeleted != true);
                
            if (parent != null)
            {
                await _context.Entry(parent)
                    .Collection(p => p.Students)
                    .Query()
                    .Where(s => s.IsDeleted != true)
                    .Include(s => s.Account)
                    .Include(s => s.Class)
                    .LoadAsync();
            }
            
            return parent;
        }

        public async Task<IEnumerable<Parent>> SearchParentsAsync(string searchTerm = "")
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllActiveParentsAsync();

            var query = _context.Parents
                .Include(p => p.Account)
                .Include(p => p.Students)
                    .ThenInclude(s => s.Account)
                .Include(p => p.Students)
                    .ThenInclude(s => s.Class)
                .Where(p => p.IsDeleted != true && p.Account.IsActive);

            query = ApplySearchFilter(query, searchTerm);
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Parent>> SortParentsAsync(string sortBy = "FullName", bool ascending = true)
        {
            var query = _context.Parents
                .Include(p => p.Account)
                .Include(p => p.Students)
                    .ThenInclude(s => s.Account)
                .Include(p => p.Students)
                    .ThenInclude(s => s.Class)
                .Where(p => p.IsDeleted != true && p.Account.IsActive);

            query = ApplySortFilter(query, sortBy, ascending);
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
                .Include(p => p.Students)
                    .ThenInclude(s => s.Class)
                .Where(p => p.IsDeleted != true && p.Account.IsActive);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = ApplySearchFilter(query, searchTerm);
            }
            var totalCount = await query.CountAsync();
            query = ApplySortFilter(query, sortBy, ascending);
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
                .Include(p => p.Students)
                    .ThenInclude(s => s.Class)
                .Where(p => p.IsDeleted != true && p.Account.IsActive)
                .AsQueryable();
        }
        private IQueryable<Parent> ApplySearchFilter(IQueryable<Parent> query, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) return query;

            var lowerTerm = searchTerm.ToLower();
            return query.Where(p =>
                (p.FullName != null && p.FullName.ToLower().Contains(lowerTerm)) ||
                p.ParentID.ToLower().Contains(lowerTerm) ||
                (p.Account != null && (
                    (p.Account.Email != null && p.Account.Email.ToLower().Contains(lowerTerm)) ||
                    p.Account.FirstName.ToLower().Contains(lowerTerm) ||
                    p.Account.LastName.ToLower().Contains(lowerTerm)
                ))
            );
        }

        private IQueryable<Parent> ApplySortFilter(IQueryable<Parent> query, string? sortBy, bool ascending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                sortBy = "FullName";

            switch (sortBy.ToLower())
            {
                case "fullname":
                    return ascending 
                        ? query.OrderBy(p => p.FullName)
                        : query.OrderByDescending(p => p.FullName);
                case "parentid":
                    return ascending 
                        ? query.OrderBy(p => p.ParentID)
                        : query.OrderByDescending(p => p.ParentID);
                case "email":
                    return ascending 
                        ? query.OrderBy(p => p.Account.Email)
                        : query.OrderByDescending(p => p.Account.Email);
                case "createdat":
                    return ascending 
                        ? query.OrderBy(p => p.Account.CreatedAt)
                        : query.OrderByDescending(p => p.Account.CreatedAt);
                default:
                    return ascending 
                        ? query.OrderBy(p => p.FullName)
                        : query.OrderByDescending(p => p.FullName);
            }
        }
    }
}
