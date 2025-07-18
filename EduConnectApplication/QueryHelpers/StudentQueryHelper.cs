using EduConnect.Domain.Entities;
using System.Linq;

namespace EduConnect.Application.QueryHelpers
{
    public static class StudentQueryHelper
    {
        public static IQueryable<Student> ApplySearch(IQueryable<Student> query, string? searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) return query;

            var lowerTerm = searchTerm.ToLower();
            return query.Where(s =>
                (s.FullName != null && s.FullName.ToLower().Contains(lowerTerm)) ||
                s.StudentID.ToLower().Contains(lowerTerm) ||
                (s.Account != null && (
                    (s.Account.Email != null && s.Account.Email.ToLower().Contains(lowerTerm)) ||
                    s.Account.FirstName.ToLower().Contains(lowerTerm) ||
                    s.Account.LastName.ToLower().Contains(lowerTerm)
                )) ||
                (s.Class != null && s.Class.ClassName != null && s.Class.ClassName.ToLower().Contains(lowerTerm)) ||
                (s.Parent != null && s.Parent.FullName != null && s.Parent.FullName.ToLower().Contains(lowerTerm))
            );
        }

        public static IQueryable<Student> ApplySort(IQueryable<Student> query, string? sortBy, bool ascending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                sortBy = "FullName";

            switch (sortBy.ToLower())
            {
                case "fullname":
                    return ascending 
                        ? query.OrderBy(s => s.FullName)
                        : query.OrderByDescending(s => s.FullName);
                case "studentid":
                    return ascending 
                        ? query.OrderBy(s => s.StudentID)
                        : query.OrderByDescending(s => s.StudentID);
                case "email":
                    return ascending 
                        ? query.OrderBy(s => s.Account.Email)
                        : query.OrderByDescending(s => s.Account.Email);
                case "createdat":
                    return ascending 
                        ? query.OrderBy(s => s.Account.CreatedAt)
                        : query.OrderByDescending(s => s.Account.CreatedAt);
                case "dateofbirth":
                    return ascending 
                        ? query.OrderBy(s => s.DateOfBirth)
                        : query.OrderByDescending(s => s.DateOfBirth);
                case "classname":
                    return ascending 
                        ? query.OrderBy(s => s.Class.ClassName)
                        : query.OrderByDescending(s => s.Class.ClassName);
                default:
                    return ascending 
                        ? query.OrderBy(s => s.FullName)
                        : query.OrderByDescending(s => s.FullName);
            }
        }
    }
}
