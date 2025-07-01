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

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(s => 
                    (s.FullName != null && s.FullName.ToLower().Contains(searchTerm)) ||
                    s.StudentID.ToLower().Contains(searchTerm) ||
                    (s.Account.Email != null && s.Account.Email.ToLower().Contains(searchTerm)) ||
                    s.Account.FirstName.ToLower().Contains(searchTerm) ||
                    s.Account.LastName.ToLower().Contains(searchTerm) ||
                    (s.Class != null && s.Class.ClassName != null && s.Class.ClassName.ToLower().Contains(searchTerm)) ||
                    (s.Parent != null && s.Parent.FullName != null && s.Parent.FullName.ToLower().Contains(searchTerm)));
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            switch (sortBy.ToLower())
            {
                case "fullname":
                    query = ascending 
                        ? query.OrderBy(s => s.FullName)
                        : query.OrderByDescending(s => s.FullName);
                    break;
                case "studentid":
                    query = ascending 
                        ? query.OrderBy(s => s.StudentID)
                        : query.OrderByDescending(s => s.StudentID);
                    break;
                case "email":
                    query = ascending 
                        ? query.OrderBy(s => s.Account.Email)
                        : query.OrderByDescending(s => s.Account.Email);
                    break;
                case "createdat":
                    query = ascending 
                        ? query.OrderBy(s => s.Account.CreatedAt)
                        : query.OrderByDescending(s => s.Account.CreatedAt);
                    break;
                case "dateofbirth":
                    query = ascending 
                        ? query.OrderBy(s => s.DateOfBirth)
                        : query.OrderByDescending(s => s.DateOfBirth);
                    break;
                case "classname":
                    query = ascending 
                        ? query.OrderBy(s => s.Class.ClassName)
                        : query.OrderByDescending(s => s.Class.ClassName);
                    break;
                default:
                    query = ascending 
                        ? query.OrderBy(s => s.FullName)
                        : query.OrderByDescending(s => s.FullName);
                    break;
            }

            // Apply pagination
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
            var student = _context.Students.Find(id);
            if (student == null)
                return Task.FromResult<Student?>(null);

            _context.Entry(student).CurrentValues.SetValues(entity);
            _context.SaveChanges();
            return Task.FromResult(student);
        }

        public async Task<List<Student>> GetStudentsByClassIdAsync(int classId)
        {
            return await _context.Students
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
    }
}
