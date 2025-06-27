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
    public class TeacherRepository : ITeacherRepository
    {
        private readonly EduConnectDbContext _context;

        public TeacherRepository(EduConnectDbContext context)
        {
            _context = context;
        }

        public async Task<Teacher> CreateAsync(Teacher entity)
        {
            await _context.Teachers.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null)
                return false;

            teacher.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Teacher>> GetAllAsync()
        {
            return await _context.Teachers
                .Include(t => t.Account)
                .Include(t => t.ClassTeachers)
                    .ThenInclude(ct => ct.Class)
                .Where(t => t.IsDeleted != true)
                .ToListAsync();
        }

        public async Task<IEnumerable<Teacher>> GetAllActiveTeachersAsync()
        {
            return await _context.Teachers
                .Include(t => t.Account)
                .Include(t => t.ClassTeachers)
                    .ThenInclude(ct => ct.Class)
                .Where(t => t.IsDeleted != true && t.Account.IsActive)
                .ToListAsync();
        }

        public async Task<Teacher?> GetByIdAsync(string id)
        {
            return await _context.Teachers
                .Include(t => t.Account)
                .Include(t => t.ClassTeachers)
                    .ThenInclude(ct => ct.Class)
                .FirstOrDefaultAsync(t => t.TeacherID == id && t.IsDeleted != true);
        }

        public async Task<Teacher?> GetTeacherWithDetailsAsync(string teacherId)
        {
            return await _context.Teachers
                .Include(t => t.Account)
                .Include(t => t.ClassTeachers)
                    .ThenInclude(ct => ct.Class)
                .FirstOrDefaultAsync(t => t.TeacherID == teacherId && t.IsDeleted != true);
        }

        public async Task<IEnumerable<Teacher>> SearchTeachersAsync(string searchTerm = "")
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllActiveTeachersAsync();

            var query = _context.Teachers
                .Include(t => t.Account)
                .Include(t => t.ClassTeachers)
                    .ThenInclude(ct => ct.Class)
                .Where(t => t.IsDeleted != true && t.Account.IsActive);

            query = ApplySearchFilter(query, searchTerm);
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Teacher>> SortTeachersAsync(string sortBy = "FullName", bool ascending = true)
        {
            var query = _context.Teachers
                .Include(t => t.Account)
                .Include(t => t.ClassTeachers)
                    .ThenInclude(ct => ct.Class)
                .Where(t => t.IsDeleted != true && t.Account.IsActive);

            query = ApplySortFilter(query, sortBy, ascending);
            return await query.ToListAsync();
        }

        public async Task<(IEnumerable<Teacher> Teachers, int TotalCount)> GetPagedTeachersAsync(
            int pageIndex, 
            int pageSize, 
            string? searchTerm = null, 
            string sortBy = "FullName", 
            bool ascending = true)
        {
            var query = _context.Teachers
                .Include(t => t.Account)
                .Include(t => t.ClassTeachers)
                    .ThenInclude(ct => ct.Class)
                .Where(t => t.IsDeleted != true && t.Account.IsActive);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = ApplySearchFilter(query, searchTerm);
            }

            var totalCount = await query.CountAsync();
            query = ApplySortFilter(query, sortBy, ascending);
            var teachers = await query
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (teachers, totalCount);
        }

        public async Task<Teacher?> UpdateAsync(string id, Teacher entity)
        {
            var existingTeacher = await _context.Teachers.FindAsync(id);
            if (existingTeacher == null)
                return null;

            existingTeacher.FullName = entity.FullName;
            existingTeacher.IsDeleted = entity.IsDeleted;

            await _context.SaveChangesAsync();
            return existingTeacher;
        }

        public IQueryable<Teacher> GetTeacherQueryable()
        {
            return _context.Teachers
                .Include(t => t.Account)
                .Include(t => t.ClassTeachers)
                    .ThenInclude(ct => ct.Class)
                .Where(t => t.IsDeleted != true && t.Account.IsActive)
                .AsQueryable();
        }
        private IQueryable<Teacher> ApplySearchFilter(IQueryable<Teacher> query, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) return query;

            var lowerTerm = searchTerm.ToLower();
            return query.Where(t =>
                (t.FullName != null && t.FullName.ToLower().Contains(lowerTerm)) ||
                t.TeacherID.ToLower().Contains(lowerTerm) ||
                (t.Account != null && (
                    (t.Account.Email != null && t.Account.Email.ToLower().Contains(lowerTerm)) ||
                    t.Account.FirstName.ToLower().Contains(lowerTerm) ||
                    t.Account.LastName.ToLower().Contains(lowerTerm)
                ))
            );
        }

        private IQueryable<Teacher> ApplySortFilter(IQueryable<Teacher> query, string? sortBy, bool ascending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                sortBy = "FullName";

            switch (sortBy.ToLower())
            {
                case "fullname":
                    return ascending 
                        ? query.OrderBy(t => t.FullName)
                        : query.OrderByDescending(t => t.FullName);
                case "teacherid":
                    return ascending 
                        ? query.OrderBy(t => t.TeacherID)
                        : query.OrderByDescending(t => t.TeacherID);
                case "email":
                    return ascending 
                        ? query.OrderBy(t => t.Account.Email)
                        : query.OrderByDescending(t => t.Account.Email);
                case "createdat":
                    return ascending 
                        ? query.OrderBy(t => t.Account.CreatedAt)
                        : query.OrderByDescending(t => t.Account.CreatedAt);
                default:
                    return ascending 
                        ? query.OrderBy(t => t.FullName)
                        : query.OrderByDescending(t => t.FullName);
            }
        }
    }
}
