using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using System.Threading.Tasks;

namespace EduConnect.Application.Services.Interface
{
    public interface IParentReportNotificationService
    {
        /// <summary>
        /// Gửi thông báo hàng ngày cho phụ huynh
        /// </summary>
        /// <returns>Kết quả thực hiện việc gửi thông báo</returns>
        Task<ApiResponse<string>> SendDailyNotificationsAsync();

        /// <summary>
        /// Gửi thông báo hàng tuần cho phụ huynh
        /// </summary>
        /// <returns>Kết quả thực hiện việc gửi thông báo</returns>
        Task<ApiResponse<string>> SendWeeklyNotificationsAsync();

        /// <summary>
        /// Gửi thông báo hàng tháng cho phụ huynh
        /// </summary>
        /// <returns>Kết quả thực hiện việc gửi thông báo</returns>
        Task<ApiResponse<string>> SendMonthlyNotificationsAsync();

        /// <summary>
        /// Gửi thông báo cho phụ huynh cụ thể theo khoảng thời gian
        /// </summary>
        /// <param name="parentId">ID của phụ huynh</param>
        /// <param name="period">Khoảng thời gian (daily, weekly, monthly)</param>
        /// <returns>Kết quả thực hiện việc gửi thông báo</returns>
        Task<ApiResponse<string>> SendNotificationToParentAsync(string parentId, string period);

        /// <summary>
        /// Lấy báo cáo phản ánh học sinh theo khoảng thời gian với phân trang và tìm kiếm
        /// </summary>
        /// <param name="request">Request chứa các tham số tìm kiếm và phân trang</param>
        /// <returns>Danh sách báo cáo phản ánh có phân trang</returns>
        Task<ApiResponse<PagedResponse<object>>> GetStudentReportsAsync(GetStudentReportsRequest request);

        /// <summary>
        /// Lấy báo cáo chi tiết của học sinh cụ thể theo khoảng thời gian
        /// </summary>
        /// <param name="studentId">ID của học sinh</param>
        /// <param name="request">Request chứa các tham số tìm kiếm</param>
        /// <returns>Báo cáo chi tiết của học sinh</returns>
        Task<ApiResponse<object>> GetStudentReportDetailsAsync(string studentId, GetStudentReportDetailsRequest request);
    }
}
