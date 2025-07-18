using EduConnect.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduConnect.Infrastruture.Repositories.Interface
{
    public interface ITimetableRepository
    {
        Task<bool> ClassExistsAsync(int classId);
        Task<bool> TeacherExistsAsync(string teacherId);
        Task<bool> SubjectExistsAsync(int subjectId);
        Task<Timetable> CreateTimetableAsync(Timetable timetable);
        Task<Timetable?> GetTimetableByIdAsync(int timetableId);
        Task<List<Timetable>> GetTimetablesByClassIdAsync(int classId, DateTime? startDate = null, DateTime? endDate = null);
        Task<bool> DeleteTimetableAsync(int timetableId);
        Task<bool> IsTimetableExistsForClassAndDateRangeAsync(int classId, DateTime startDate, DateTime endDate);
    }
}