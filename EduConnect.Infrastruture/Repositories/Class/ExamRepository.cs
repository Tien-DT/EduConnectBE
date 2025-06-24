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
    public class ExamRepository : BaseRepository<Exam, int>, IExamRepository
    {
        private readonly EduConnectDbContext _context;
        public ExamRepository(EduConnectDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task CreateExamAsync(Exam exam)
        {
            await _context.Exams.AddAsync(exam);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteExamAsync(int id)
        {
            var exam = await _context.Exams.FindAsync(id);
            if (exam != null)
            {
                exam.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Exam>> GetExamByStudentIdAsync(string studentId, DateTime? startDate = null, DateTime? endDate = null, bool ascending = false)
        {
            var query = _context.Exams
                .Where(e => e.StudentID == studentId && e.IsDeleted != true);
            if (startDate.HasValue)
            {
                query = query.Where(e => e.ExamDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(e => e.ExamDate <= endDate.Value);
            }
            query = query.Include(e => e.Subject);

            if (ascending)
            {
                query = query.OrderBy(e => e.ExamDate);
            }
            else
            {
                query = query.OrderByDescending(e => e.ExamDate);
            }

            return await query.ToListAsync();
        }

        public async Task<(List<Exam> items, int totalCount)> GetPagedExamByStudentIdAsync(
            string studentId,
            int pageIndex,
            int pageSize,
            DateTime? startDate = null,
            DateTime? endDate = null,
            bool ascending = false)
        {
            var query = _context.Exams
                .Where(e => e.StudentID == studentId && e.IsDeleted != true);
            if (startDate.HasValue)
            {
                query = query.Where(e => e.ExamDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(e => e.ExamDate <= endDate.Value);
            }
            query = query.Include(e => e.Subject);

            var totalCount = await query.CountAsync();

            if (ascending)
            {
                query = query.OrderBy(e => e.ExamDate);
            }
            else
            {
                query = query.OrderByDescending(e => e.ExamDate);
            }
            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<List<Exam>> GetExamsByAccountIdAsync(string accountId, DateTime? startDate = null, DateTime? endDate = null, bool ascending = false)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.AccountID == accountId);

            if (student == null)
                return new List<Exam>();

            var query = _context.Exams
                .Where(e => e.StudentID == student.StudentID && e.IsDeleted != true);

            if (startDate.HasValue)
            {
                query = query.Where(e => e.ExamDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(e => e.ExamDate <= endDate.Value);
            }
            query = query.Include(e => e.Subject);

            if (ascending)
            {
                query = query.OrderBy(e => e.ExamDate);
            }
            else
            {
                query = query.OrderByDescending(e => e.ExamDate);
            }

            return await query.ToListAsync();
        }

        public async Task<(List<Exam> items, int totalCount)> GetPagedExamsByAccountIdAsync(
            string accountId,
            int pageIndex,
            int pageSize,
            DateTime? startDate = null,
            DateTime? endDate = null,
            bool ascending = false)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.AccountID == accountId);

            if (student == null)
                return (new List<Exam>(), 0);

            var query = _context.Exams
                .Where(e => e.StudentID == student.StudentID && e.IsDeleted != true);
            if (startDate.HasValue)
            {
                query = query.Where(e => e.ExamDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(e => e.ExamDate <= endDate.Value);
            }
            query = query.Include(e => e.Subject);
            var totalCount = await query.CountAsync();
            if (ascending)
            {
                query = query.OrderBy(e => e.ExamDate);
            }
            else
            {
                query = query.OrderByDescending(e => e.ExamDate);
            }
            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task UpdateExamAsync(Exam exam)
        {
            _context.Exams.Update(exam);
            await _context.SaveChangesAsync();
        }

        public IQueryable<Exam> GetExamQueryable()
        {
            return _context.Exams.AsQueryable();
        }
    }
}
