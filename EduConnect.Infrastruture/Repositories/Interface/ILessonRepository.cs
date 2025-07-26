using EduConnect.Domain.Entities;

namespace EduConnect.Infrastruture.Repositories.Interface;

public interface ILessonRepository : IBaseRepository<Lesson, int>
{
    Task<IEnumerable<Lesson>> GetLessonsByTeacherIdAsync(string teacherId, int pageIndex, int pageSize);
    Task<int> CountLessonsByTeacherIdAsync(string teacherId);
    Task<Lesson?> GetLessonByIdAndTeacherIdAsync(int lessonId, string teacherId);
    Task<IEnumerable<Lesson>> GetLessonsByTeacherAndDateRangeAsync(string teacherId, DateTime startDate, DateTime endDate);
}
