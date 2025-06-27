using EduConnect.Domain.Entities;
using System.Linq;

namespace EduConnect.Application.QueryHelpers
{
    public static class ClassQueryHelper
    {
        public static IQueryable<Class> ApplySearch(IQueryable<Class> query, string? searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) return query;

            var lowerTerm = searchTerm.ToLower();
            return query.Where(c =>
                (c.ClassName != null && c.ClassName.ToLower().Contains(lowerTerm))
            );
        }

        public static IQueryable<Class> ApplySort(IQueryable<Class> query, string? sortBy, bool ascending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                sortBy = "classname";

            switch (sortBy.ToLower())
            {
                case "classname":
                    return ascending 
                        ? query.OrderBy(c => c.ClassName)
                        : query.OrderByDescending(c => c.ClassName);
                case "classid":
                    return ascending 
                        ? query.OrderBy(c => c.ClassID)
                        : query.OrderByDescending(c => c.ClassID);
                default:
                    return ascending 
                        ? query.OrderBy(c => c.ClassName)
                        : query.OrderByDescending(c => c.ClassName);
            }
        }
    }
}
