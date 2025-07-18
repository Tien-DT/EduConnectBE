using EduConnect.Domain.Entities;
using EduConnect.Data;
using EduConnect.Infrastruture.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduConnect.Infrastruture.Repositories.Class
{
    public class StudentLessonNoteRepository : BaseRepository<StudentLessonNote, int>, IStudentLessonNoteRepository
    {
        private readonly EduConnectDbContext _dbContext;

        public StudentLessonNoteRepository(EduConnectDbContext context) : base(context)
        {
            _dbContext = context;
        }

        public async Task<IEnumerable<StudentLessonNote>> GetByLessonReportIdAsync(int lessonReportId)
        {
            return await _dbContext.StudentLessonNotes
                .Where(sln => sln.LessonReportID == lessonReportId && sln.IsDeleted != true)
                .Include(sln => sln.Student)
                .Include(sln => sln.LessonReport)
                .ToListAsync();
        }

        public async Task<IEnumerable<StudentLessonNote>> GetByStudentIdAsync(string studentId)
        {
            return await _dbContext.StudentLessonNotes
                .Where(sln => sln.StudentID == studentId && sln.IsDeleted != true)
                .Include(sln => sln.Student)
                .Include(sln => sln.LessonReport)
                .ToListAsync();
        }
    }
}
