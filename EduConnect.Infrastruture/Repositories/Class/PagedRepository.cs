using EduConnect.Infrastruture.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduConnect.Infrastruture.Repositories.Class
{
    public class PagedRepository<T> : List<T>
    {
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }

        public PagedRepository(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            AddRange(items);
        }

        public static async Task<PagedRepository<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();

            if (pageIndex <= 0)
            {
                throw new ArgumentOutOfRangeException("pageNumber cannot be less than or equal to zero.");
            }

            var items = await source.Skip((pageIndex - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();

            int totalPages = (int)Math.Ceiling(count / (double)pageSize);

            if (pageIndex > totalPages && totalPages > 0)
            {
                throw new ArgumentOutOfRangeException($"Page {pageIndex} does not exist. Total pages: {totalPages}");
            }

            return new PagedRepository<T>(items, count, pageIndex, pageSize);
        }

        public static async Task<PagedRepository<T>> CreateAsync(List<T> source, int pageIndex, int pageSize)
        {
            var count = source.Count;

            if (pageIndex <= 0)
            {
                throw new ArgumentOutOfRangeException("pageNumber cannot be less than or equal to zero.");
            }

            var items = source.Skip((pageIndex - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToList();

            int totalPages = (int)Math.Ceiling(count / (double)pageSize);

            if (pageIndex > totalPages && totalPages > 0)
            {
                throw new ArgumentOutOfRangeException($"Page {pageIndex} does not exist. Total pages: {totalPages}");
            }

            return new PagedRepository<T>(items, count, pageIndex, pageSize);
        }

        public static async Task<PagedRepository<T>> CreateOrderedQueryAsync(IOrderedQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();

            if (pageIndex <= 0)
            {
                throw new ArgumentOutOfRangeException("pageNumber cannot be less than or equal to zero.");
            }

            var items = source.Skip((pageIndex - 1) * pageSize)
                              .Take(pageSize)
                              .ToList();

            int totalPages = (int)Math.Ceiling(count / (double)pageSize);

            if (pageIndex > totalPages && totalPages > 0)
            {
                throw new ArgumentOutOfRangeException($"Page {pageIndex} does not exist. Total pages: {totalPages}");
            }

            return new PagedRepository<T>(items, count, pageIndex, pageSize);
        }
    }
}
