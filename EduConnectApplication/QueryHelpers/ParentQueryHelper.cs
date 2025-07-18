using EduConnect.Domain.Entities;
using System.Linq;

namespace EduConnect.Application.QueryHelpers
{
    public static class ParentQueryHelper
    {
        public static IQueryable<Parent> ApplySearch(IQueryable<Parent> query, string? searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) return query;

            var lowerTerm = searchTerm.ToLower();
            return query.Where(p =>
                (p.FullName != null && p.FullName.ToLower().Contains(lowerTerm)) ||
                p.ParentID.ToLower().Contains(lowerTerm) ||
                (p.Account != null && (
                    (p.Account.Email != null && p.Account.Email.ToLower().Contains(lowerTerm)) ||
                    p.Account.FirstName.ToLower().Contains(lowerTerm) ||
                    p.Account.LastName.ToLower().Contains(lowerTerm)
                ))
            );
        }

        public static IQueryable<Parent> ApplySort(IQueryable<Parent> query, string? sortBy, bool ascending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                sortBy = "FullName";

            switch (sortBy.ToLower())
            {
                case "fullname":
                    return ascending 
                        ? query.OrderBy(p => p.FullName)
                        : query.OrderByDescending(p => p.FullName);
                case "parentid":
                    return ascending 
                        ? query.OrderBy(p => p.ParentID)
                        : query.OrderByDescending(p => p.ParentID);
                case "email":
                    return ascending 
                        ? query.OrderBy(p => p.Account.Email)
                        : query.OrderByDescending(p => p.Account.Email);
                case "createdat":
                    return ascending 
                        ? query.OrderBy(p => p.Account.CreatedAt)
                        : query.OrderByDescending(p => p.Account.CreatedAt);
                default:
                    return ascending 
                        ? query.OrderBy(p => p.FullName)
                        : query.OrderByDescending(p => p.FullName);
            }
        }
    }
}
