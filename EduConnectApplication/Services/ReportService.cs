using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EduConnect.Domain.Entities;
using EduConnect.Data;
using EduConnectApplication.DTOs.Request;
using EduConnectApplication.DTOs.Response;
using EduConnect.Application.DTOs.Response;
using EduConnectApplication.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace EduConnectApplication.Services
{
    public class ReportService : IReportService
    {
        private readonly EduConnectDbContext _context;
        public ReportService(EduConnectDbContext context)
        {
            _context = context;
        }

        public async Task<ReportResponse> CreateReportAsync(CreateReportRequest request)
        {
            var report = new Report
            {
                AccountId = request.AccountId,
                ReportType = request.ReportType,
                Content = request.Content,
                Status = request.Status,
                CreatedBy = request.CreatedBy,
                CreatedDate = System.DateTime.UtcNow,
                IsDelete = false
            };
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
            return ToResponse(report);
        }

        public async Task<ReportResponse> UpdateReportAsync(UpdateReportRequest request)
        {
            var report = await _context.Reports.FindAsync(request.ReportId);
            if (report == null) throw new KeyNotFoundException("Report not found");
            report.ReportType = request.ReportType;
            report.Content = request.Content;
            report.Status = request.Status;
            report.IsDelete = request.IsDelete;
            await _context.SaveChangesAsync();
            return ToResponse(report);
        }

        public async Task<bool> DeleteReportAsync(int reportId)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null) return false;
            report.IsDelete = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ReportResponse?> GetReportByIdAsync(int reportId)
        {
            var report = await _context.Reports.AsNoTracking().FirstOrDefaultAsync(r => r.ReportId == reportId);
            return report == null ? null : ToResponse(report);
        }

        public async Task<PagedResponse<ReportResponse>> GetReportsAsync(GetReportRequest request)
        {
            var query = _context.Reports.AsNoTracking().Where(r => !r.IsDelete);

            if (!string.IsNullOrEmpty(request.AccountId))
                query = query.Where(r => r.AccountId == request.AccountId);
            if (!string.IsNullOrEmpty(request.ReportType))
                query = query.Where(r => r.ReportType == request.ReportType);
            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(r => r.Status == request.Status);
            if (!string.IsNullOrEmpty(request.Search))
                query = query.Where(r => r.Content.Contains(request.Search));

            // Sort
            if (!string.IsNullOrEmpty(request.SortBy))
            {
                if (request.SortBy.ToLower() == "createddate")
                    query = request.SortDesc ? query.OrderByDescending(r => r.CreatedDate) : query.OrderBy(r => r.CreatedDate);
                else if (request.SortBy.ToLower() == "reporttype")
                    query = request.SortDesc ? query.OrderByDescending(r => r.ReportType) : query.OrderBy(r => r.ReportType);
                else if (request.SortBy.ToLower() == "status")
                    query = request.SortDesc ? query.OrderByDescending(r => r.Status) : query.OrderBy(r => r.Status);
                else
                    query = request.SortDesc ? query.OrderByDescending(r => r.ReportId) : query.OrderBy(r => r.ReportId);
            }

            var totalCount = await query.CountAsync();
            var items = await query.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync();

            return new PagedResponse<ReportResponse>
            {
                Items = items.Select(ToResponse).ToList(),
                TotalCount = totalCount,
                PageIndex = request.Page,
                PageSize = request.PageSize
            };
        }

        private static ReportResponse ToResponse(Report report)
        {
            return new ReportResponse
            {
                ReportId = report.ReportId,
                AccountId = report.AccountId,
                ReportType = report.ReportType,
                Content = report.Content,
                Status = report.Status,
                IsDelete = report.IsDelete,
                CreatedBy = report.CreatedBy,
                CreatedDate = report.CreatedDate
            };
        }
    }
}
