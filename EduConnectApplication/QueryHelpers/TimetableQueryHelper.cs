using EduConnect.Domain.Entities;
using System;
using System.Linq;

namespace EduConnect.Application.QueryHelpers
{
    public static class TimetableQueryHelper
    {
        public static IQueryable<Timetable> ApplyDateFilter(IQueryable<Timetable> query, DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue)
            {
                query = query.Where(t => t.EndDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(t => t.StartDate <= endDate.Value);
            }

            return query;
        }

        public static IQueryable<Timetable> ApplySort(IQueryable<Timetable> query, string? sortBy, bool ascending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                sortBy = "startdate";

            switch (sortBy.ToLower())
            {
                case "startdate":
                    return ascending 
                        ? query.OrderBy(t => t.StartDate)
                        : query.OrderByDescending(t => t.StartDate);
                case "enddate":
                    return ascending 
                        ? query.OrderBy(t => t.EndDate)
                        : query.OrderByDescending(t => t.EndDate);
                case "classname":
                    return ascending 
                        ? query.OrderBy(t => t.Class.ClassName)
                        : query.OrderByDescending(t => t.Class.ClassName);
                default:
                    return ascending 
                        ? query.OrderBy(t => t.StartDate)
                        : query.OrderByDescending(t => t.StartDate);
            }
        }
    }
}
