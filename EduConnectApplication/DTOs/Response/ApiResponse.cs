using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduConnect.Application.DTOs.Response
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public List<String>? Error { get; set; }        

        public static ApiResponse<T> SuccessResponse(T data, string message)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                Error = null
            };
        }

        public static ApiResponse<T> ErrorResponse(List<string> errors, string errorMessage)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = errorMessage,
                Data = default,
                Error = errors,
            };
        }
    }

    public class PagedApiResponse<T> : ApiResponse<T>
    {
        public int TotalCount { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }

        public static PagedApiResponse<T> SuccessPagedResponse(T data, string message, 
            int totalCount, int pageIndex, int pageSize)
        {
            return new PagedApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                Error = null,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }

        public static PagedApiResponse<T> ErrorPagedResponse(List<string> errors, string errorMessage,
            int pageIndex = 0, int pageSize = 0)
        {
            return new PagedApiResponse<T>
            {
                Success = false,
                Message = errorMessage,
                Data = default,
                Error = errors,
                TotalCount = 0,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalPages = 0
            };
        }
    }
}
