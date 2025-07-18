using EduConnect.Domain.Entities;
using System.Linq;

namespace EduConnect.Application.QueryHelpers
{
    public static class ExamQueryHelper
    {
        public static IQueryable<Exam> ApplySearch(IQueryable<Exam> query, string? searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) return query;
            var lowerTerm = searchTerm.ToLower();
            return query.Where(e =>
                (e.ExamNote != null && e.ExamNote.ToLower().Contains(lowerTerm)) ||
                (e.ExamType != null && e.ExamType.ToLower().Contains(lowerTerm)) ||
                (e.Subject != null && (
                    (e.Subject.SubjectName != null && e.Subject.SubjectName.ToLower().Contains(lowerTerm)) ||
                    (e.Subject.SubjectCode != null && e.Subject.SubjectCode.ToLower().Contains(lowerTerm))
                )) ||
                (e.StudentID != null && e.StudentID.ToLower().Contains(lowerTerm))
            );
        }

        public static IQueryable<Exam> ApplyRoomFilter(IQueryable<Exam> query, string? room)
        {
            if (string.IsNullOrWhiteSpace(room)) return query;
            var lowerRoom = room.ToLower();
            return query.Where(e => e.Room != null && e.Room.ToLower().Contains(lowerRoom));
        }

        public static IQueryable<Exam> ApplySort(IQueryable<Exam> query, string? sortBy, bool ascending)
        {
            if (string.IsNullOrWhiteSpace(sortBy) || sortBy.ToLower() == "examdate")
                return ascending ? query.OrderBy(e => e.ExamDate) : query.OrderByDescending(e => e.ExamDate);
            if (sortBy.ToLower() == "examtype")
                return ascending ? query.OrderBy(e => e.ExamType) : query.OrderByDescending(e => e.ExamType);
            if (sortBy.ToLower() == "room")
                return ascending ? query.OrderBy(e => e.Room) : query.OrderByDescending(e => e.Room);
            return query;
        }
    }
}
