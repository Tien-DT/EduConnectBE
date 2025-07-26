using EduConnect.Data;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduConnect.Infrastruture.Repositories.Class
{
    public class StudentRepository: IBaseRepository<Student, string>, IStudentRepository
    {
        private readonly EduConnectDbContext _context;
        public StudentRepository(EduConnectDbContext context)
        {
            _context = context;
        }
        public Task<Student> CreateAsync(Student entity)
        {
            _context.Students.Add(entity);
            _context.SaveChanges();
            return Task.FromResult(entity);
        }

        public Task<bool> DeleteAsync(string id)
        {
            var student = _context.Students.Find(id);
            if (student == null)
            {
                return Task.FromResult(false);
            }
            _context.Students.Remove(student);
            _context.SaveChanges();
            return Task.FromResult(true);
        }

        public Task<IEnumerable<Student>> GetAllAsync()
        {
            var students = _context.Students
                .Include(s => s.Account)
                .Include(s => s.Class)
                .Include(s => s.Parent)
                .Where(s => s.IsDeleted != true)
                .ToList();
            return Task.FromResult<IEnumerable<Student>>(students);
        }

        public async Task<IEnumerable<Student>> GetAllActiveStudentsAsync()
        {
            return await _context.Students
                .Include(s => s.Account)
                .Include(s => s.Class)
                .Include(s => s.Parent)
                .Where(s => s.IsDeleted != true && s.Account.IsActive)
                .ToListAsync();
        }

        public async Task<Student?> GetStudentWithDetailsAsync(string studentId)
        {
            return await _context.Students
                .Include(s => s.Account)
                .Include(s => s.Class)
                .Include(s => s.Parent)
                .FirstOrDefaultAsync(s => s.StudentID == studentId && s.IsDeleted != true);
        }

        public async Task<(IEnumerable<Student> Students, int TotalCount)> GetPagedStudentsAsync(
            int pageIndex, 
            int pageSize, 
            string? searchTerm = null, 
            string sortBy = "FullName", 
            bool ascending = true)
        {
            var query = _context.Students
                .Include(s => s.Account)
                .Include(s => s.Class)
                .Include(s => s.Parent)
                .Where(s => s.IsDeleted != true && s.Account.IsActive);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = ApplySearchFilter(query, searchTerm);
            }
            var totalCount = await query.CountAsync();
            query = ApplySortFilter(query, sortBy, ascending);
            var students = await query
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (students, totalCount);
        }

        public IQueryable<Student> GetStudentQueryable()
        {
            return _context.Students
                .Include(s => s.Account)
                .Include(s => s.Class)
                .Include(s => s.Parent)
                .Where(s => s.IsDeleted != true && s.Account.IsActive)
                .AsQueryable();
        }

        public Task<Student?> GetByIdAsync(string id)
        {
            var student = _context.Students.Find(id);
            return Task.FromResult(student);
        }

        public Task<Student?> UpdateAsync(string id, Student entity)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Student>> GetStudentsByClassIdAsync(int classId)
        {
            return await _context.Students
                .Include(s => s.Parent)
                .Include(s => s.Class)
                .Where(s => s.ClassID == classId && (s.IsDeleted == null || s.IsDeleted == false))
                .ToListAsync();
        }

        public async Task<List<Student>> GetStudentsByIdsAsync(List<string> studentIds)
        {
            return await _context.Students
                .Where(s => studentIds.Contains(s.StudentID) && (s.IsDeleted == null || s.IsDeleted == false))
                .ToListAsync();
        }

        public async Task<Timetable?> GetLatestTimetableByClassIdAsync(int classId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Timetables
                .Include(t => t.TimetableSlots)
                    .ThenInclude(slot => slot.Subject)
                .Include(t => t.TimetableSlots)
                    .ThenInclude(slot => slot.Teacher)
                .Where(t => t.ClassID == classId && (t.IsDeleted == null || t.IsDeleted == false));
            if (startDate.HasValue)
                query = query.Where(t => t.StartDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(t => t.EndDate <= endDate.Value);
            return await query.OrderByDescending(t => t.StartDate).FirstOrDefaultAsync();
        }

        public async Task<List<Timetable>> GetAllTimetablesByClassIdAsync(int classId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Timetables
                .Include(t => t.TimetableSlots)
                    .ThenInclude(slot => slot.Subject)
                .Include(t => t.TimetableSlots)
                    .ThenInclude(slot => slot.Teacher)
                .Where(t => t.ClassID == classId && (t.IsDeleted == null || t.IsDeleted == false));
            
            if (startDate.HasValue)
                query = query.Where(t => t.EndDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(t => t.StartDate <= endDate.Value);
            
            return await query.OrderByDescending(t => t.StartDate).ToListAsync();
        }

        private IQueryable<Student> ApplySearchFilter(IQueryable<Student> query, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) return query;

            var lowerTerm = searchTerm.ToLower();
            return query.Where(s =>
                (s.FullName != null && s.FullName.ToLower().Contains(lowerTerm)) ||
                s.StudentID.ToLower().Contains(lowerTerm) ||
                (s.Account != null && (
                    (s.Account.Email != null && s.Account.Email.ToLower().Contains(lowerTerm)) ||
                    s.Account.FirstName.ToLower().Contains(lowerTerm) ||
                    s.Account.LastName.ToLower().Contains(lowerTerm)
                )) ||
                (s.Class != null && s.Class.ClassName != null && s.Class.ClassName.ToLower().Contains(lowerTerm)) ||
                (s.Parent != null && s.Parent.FullName != null && s.Parent.FullName.ToLower().Contains(lowerTerm))
            );
        }

        private IQueryable<Student> ApplySortFilter(IQueryable<Student> query, string? sortBy, bool ascending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                sortBy = "FullName";

            switch (sortBy.ToLower())
            {
                case "fullname":
                    return ascending 
                        ? query.OrderBy(s => s.FullName)
                        : query.OrderByDescending(s => s.FullName);
                case "studentid":
                    return ascending 
                        ? query.OrderBy(s => s.StudentID)
                        : query.OrderByDescending(s => s.StudentID);
                case "email":
                    return ascending 
                        ? query.OrderBy(s => s.Account.Email)
                        : query.OrderByDescending(s => s.Account.Email);
                case "createdat":
                    return ascending 
                        ? query.OrderBy(s => s.Account.CreatedAt)
                        : query.OrderByDescending(s => s.Account.CreatedAt);
                case "dateofbirth":
                    return ascending 
                        ? query.OrderBy(s => s.DateOfBirth)
                        : query.OrderByDescending(s => s.DateOfBirth);
                case "classname":
                    return ascending 
                        ? query.OrderBy(s => s.Class.ClassName)
                        : query.OrderByDescending(s => s.Class.ClassName);
                default:
                    return ascending 
                        ? query.OrderBy(s => s.FullName)
                        : query.OrderByDescending(s => s.FullName);
            }
        }

        public async Task<Student?> GetByAccountIdAsync(string accountId)
        {
            return await _context.Students
                .FirstOrDefaultAsync(s => s.AccountID == accountId && (s.IsDeleted == null || s.IsDeleted == false));
        }

        public async Task<List<Student>> GetStudentsWithoutClassAsync()
        {
            return await _context.Students
                .Include(s => s.Account)
                .Include(s => s.Parent)
                .Where(s => s.ClassID == null && (s.IsDeleted == null || s.IsDeleted == false))
                .ToListAsync();
        }

        public async Task<bool> AddStudentsToClassAsync(int classId, List<string> studentIds)
        {
            try
            {
                var students = await _context.Students
                    .Where(s => studentIds.Contains(s.StudentID) && (s.IsDeleted == null || s.IsDeleted == false))
                    .ToListAsync();

                foreach (var student in students)
                {
                    student.ClassID = classId;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveStudentsFromClassAsync(List<string> studentIds)
        {
            try
            {
                var students = await _context.Students
                    .Where(s => studentIds.Contains(s.StudentID) && (s.IsDeleted == null || s.IsDeleted == false))
                    .ToListAsync();

                foreach (var student in students)
                {
                    student.ClassID = null;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateStudentClassAsync(string studentId, int? newClassId)
        {
            try
            {
                var student = await _context.Students
                    .FirstOrDefaultAsync(s => s.StudentID == studentId && (s.IsDeleted == null || s.IsDeleted == false));

                if (student == null)
                    return false;

                student.ClassID = newClassId;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Student?> GetStudentWithParentAsync(string studentId)
        {
            return await _context.Students
                .Include(s => s.Parent)
                    .ThenInclude(p => p.Account)
                .Include(s => s.Class)
                .FirstOrDefaultAsync(s => s.StudentID == studentId && s.IsDeleted != true);
        }
    }
}
