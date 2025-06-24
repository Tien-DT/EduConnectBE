using System.Collections.Generic;

namespace EduConnect.Application.DTOs.Response
{
    public class PagedResponse<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)System.Math.Ceiling((double)TotalCount / PageSize);
    }
}
