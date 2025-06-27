using EduConnect.Domain.Entities;

namespace EduConnect.Application.QueryHelpers
{
    public static class ParentFieldMetadata
    {
        public static readonly string[] SearchableFields = new[]
        {
            nameof(Parent.FullName),
            nameof(Parent.ParentID),
            "Account.Email",
            "Account.FirstName",
            "Account.LastName"
        };

        public static readonly string[] SortableFields = new[]
        {
            nameof(Parent.FullName),
            nameof(Parent.ParentID),
            "Account.Email",
            "Account.CreatedAt"
        };
    }
}
