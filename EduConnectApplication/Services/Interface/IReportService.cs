using System.Collections.Generic;
using System.Threading.Tasks;
using EduConnectApplication.DTOs.Request;
using EduConnectApplication.DTOs.Response;
using EduConnect.Application.DTOs.Response;
using EduConnectApplication.DTOs.Response;

namespace EduConnectApplication.Services.Interface
{
    public interface IReportService
    {
        Task<ReportResponse> CreateReportAsync(CreateReportRequest request);
        Task<ReportResponse> UpdateReportAsync(UpdateReportRequest request);
        Task<bool> DeleteReportAsync(int reportId);
        Task<ReportResponse?> GetReportByIdAsync(int reportId);
        Task<PagedResponse<ReportResponse>> GetReportsAsync(GetReportRequest request);
    }
}
