using EduConnect.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduConnect.Infrastruture.Repositories.Interface
{
    public interface IStudentLessonNoteRepository : IBaseRepository<StudentLessonNote, int>
    {
        Task<IEnumerable<StudentLessonNote>> GetByLessonReportIdAsync(int lessonReportId);
        Task<IEnumerable<StudentLessonNote>> GetByStudentIdAsync(string studentId);
    }
}
