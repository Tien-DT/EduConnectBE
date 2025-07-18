using EduConnect.Domain.Entities;

namespace EduConnect.Application.QueryHelpers
{
    public static class ExamFieldMetadata
    {
        public static readonly string[] SearchableFields = new[]
        {
            nameof(Exam.ExamNote),
            nameof(Exam.ExamType),
            nameof(Exam.Room),
            "Subject.SubjectName",
            "Subject.SubjectCode",
            nameof(Exam.StudentID)
        };

        public static readonly string[] SortableFields = new[]
        {
            nameof(Exam.ExamDate),
            nameof(Exam.ExamType),
            nameof(Exam.Room)
        };
    }
}
