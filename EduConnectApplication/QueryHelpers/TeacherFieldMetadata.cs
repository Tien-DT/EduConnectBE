using EduConnect.Domain.Entities;

namespace EduConnect.Application.QueryHelpers
{
    public static class TeacherFieldMetadata
    {
        public static readonly string[] SearchableFields = new[]
        {
            nameof(Teacher.FullName),
            nameof(Teacher.TeacherID),
            "Account.Email",
            "Account.FirstName",
            "Account.LastName"
        };

        public static readonly string[] SortableFields = new[]
        {
            nameof(Teacher.FullName),
            nameof(Teacher.TeacherID),
            "Account.Email",
            "Account.CreatedAt"
        };
    }
}
