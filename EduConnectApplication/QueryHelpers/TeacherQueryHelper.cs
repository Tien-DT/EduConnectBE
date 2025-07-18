using EduConnect.Domain.Entities;
using System.Linq;

namespace EduConnect.Application.QueryHelpers
{
    public static class TeacherQueryHelper
    {
        public static IQueryable<Teacher> ApplySearch(IQueryable<Teacher> query, string? searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) return query;

            var lowerTerm = searchTerm.ToLower();
            return query.Where(t =>
                (t.FullName != null && t.FullName.ToLower().Contains(lowerTerm)) ||
                t.TeacherID.ToLower().Contains(lowerTerm) ||
                (t.Account != null && (
                    (t.Account.Email != null && t.Account.Email.ToLower().Contains(lowerTerm)) ||
                    t.Account.FirstName.ToLower().Contains(lowerTerm) ||
                    t.Account.LastName.ToLower().Contains(lowerTerm)
                ))
            );
        }

        public static IQueryable<Teacher> ApplySort(IQueryable<Teacher> query, string? sortBy, bool ascending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                sortBy = "FullName";

            switch (sortBy.ToLower())
            {
                case "fullname":
                    return ascending 
                        ? query.OrderBy(t => t.FullName)
                        : query.OrderByDescending(t => t.FullName);
                case "teacherid":
                    return ascending 
                        ? query.OrderBy(t => t.TeacherID)
                        : query.OrderByDescending(t => t.TeacherID);
                case "email":
                    return ascending 
                        ? query.OrderBy(t => t.Account.Email)
                        : query.OrderByDescending(t => t.Account.Email);
                case "createdat":
                    return ascending 
                        ? query.OrderBy(t => t.Account.CreatedAt)
                        : query.OrderByDescending(t => t.Account.CreatedAt);
                default:
                    return ascending 
                        ? query.OrderBy(t => t.FullName)
                        : query.OrderByDescending(t => t.FullName);
            }
        }
    }
}
