using EduConnect.Data;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduConnect.Infrastruture.Repositories.Class
{
    public class SubjectRepository : ISubjectRepository
    {
        private readonly EduConnectDbContext _context;

        public SubjectRepository(EduConnectDbContext context)
        {
            _context = context;
        }

        public async Task<Subject?> GetByIdAsync(int subjectId)
        {
            return await _context.Subjects
                .Include(s => s.Lessons)
                .Include(s => s.Exams)
                .FirstOrDefaultAsync(s => s.SubjectId == subjectId && (s.IsDeleted == null || s.IsDeleted == false));
        }

        public async Task<List<Subject>> GetAllAsync()
        {
            return await _context.Subjects
                .Include(s => s.Lessons)
                .Include(s => s.Exams)
                .Where(s => s.IsDeleted == null || s.IsDeleted == false)
                .OrderBy(s => s.SubjectName)
                .ToListAsync();
        }

        public async Task<Subject> AddAsync(Subject subject)
        {
            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();
            return subject;
        }

        public async Task<Subject> UpdateAsync(Subject subject)
        {
            _context.Subjects.Update(subject);
            await _context.SaveChangesAsync();
            return subject;
        }

        public async Task<bool> DeleteAsync(int subjectId)
        {
            var subject = await GetByIdAsync(subjectId);
            if (subject == null) return false;

            subject.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int subjectId)
        {
            return await _context.Subjects
                .AnyAsync(s => s.SubjectId == subjectId && (s.IsDeleted == null || s.IsDeleted == false));
        }

        public async Task<bool> ExistsByCodeAsync(string subjectCode)
        {
            return await _context.Subjects
                .AnyAsync(s => s.SubjectCode == subjectCode && (s.IsDeleted == null || s.IsDeleted == false));
        }

        public async Task<bool> ExistsByNameAsync(string subjectName)
        {
            return await _context.Subjects
                .AnyAsync(s => s.SubjectName == subjectName && (s.IsDeleted == null || s.IsDeleted == false));
        }

        public async Task<(List<Subject> Items, int TotalCount)> FilterSortSearchAsync(
            string? searchTerm = null,
            string? sortBy = null,
            bool ascending = true,
            int pageIndex = 1,
            int pageSize = 10,
            bool includeDeleted = false)
        {
            var query = _context.Subjects
                .Include(s => s.Lessons)
                .Include(s => s.Exams)
                .AsQueryable();

            // Filter by deleted status
            if (!includeDeleted)
            {
                query = query.Where(s => s.IsDeleted == null || s.IsDeleted == false);
            }

            // Search
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(s => 
                    s.SubjectName.Contains(searchTerm) ||
                    (s.SubjectCode != null && s.SubjectCode.Contains(searchTerm)) ||
                    (s.Description != null && s.Description.Contains(searchTerm)));
            }

            // Get total count before paging
            var totalCount = await query.CountAsync();

            // Sort
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "subjectname":
                        query = ascending ? query.OrderBy(s => s.SubjectName) : query.OrderByDescending(s => s.SubjectName);
                        break;
                    case "subjectcode":
                        query = ascending ? query.OrderBy(s => s.SubjectCode) : query.OrderByDescending(s => s.SubjectCode);
                        break;
                    case "description":
                        query = ascending ? query.OrderBy(s => s.Description) : query.OrderByDescending(s => s.Description);
                        break;
                    default:
                        query = ascending ? query.OrderBy(s => s.SubjectName) : query.OrderByDescending(s => s.SubjectName);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(s => s.SubjectName);
            }

            // Paging
            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
