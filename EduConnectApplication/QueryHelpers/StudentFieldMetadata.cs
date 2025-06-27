using EduConnect.Domain.Entities;

namespace EduConnect.Application.QueryHelpers
{
    public static class StudentFieldMetadata
    {
        public static readonly string[] SearchableFields = new[]
        {
            nameof(Student.FullName),
            nameof(Student.StudentID),
            "Account.Email",
            "Account.FirstName",
            "Account.LastName",
            "Class.ClassName",
            "Parent.FullName"
        };

        public static readonly string[] SortableFields = new[]
        {
            nameof(Student.FullName),
            nameof(Student.StudentID),
            "Account.Email",
            "Account.CreatedAt",
            nameof(Student.DateOfBirth),
            "Class.ClassName"
        };
    }
}
