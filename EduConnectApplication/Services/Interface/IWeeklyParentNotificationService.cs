using EduConnect.Application.DTOs.Response;
using System.Threading.Tasks;

namespace EduConnect.Application.Services.Interface
{
    public interface IWeeklyParentNotificationService
    {
        /// <summary>
        /// Gửi thông báo hàng tuần cho phụ huynh về các phản ánh của học sinh trong tuần
        /// </summary>
        /// <returns>Kết quả thực hiện việc gửi thông báo</returns>
        Task<ApiResponse<string>> SendWeeklyNotificationsAsync();

        /// <summary>
        /// Gửi thông báo cho phụ huynh cụ thể về các phản ánh của con em họ trong tuần
        /// </summary>
        /// <param name="parentId">ID của phụ huynh</param>
        /// <returns>Kết quả thực hiện việc gửi thông báo</returns>
        Task<ApiResponse<string>> SendWeeklyNotificationToParentAsync(string parentId);

        /// <summary>
        /// Lấy tóm tắt các phản ánh trong tuần của tất cả học sinh
        /// </summary>
        /// <returns>Danh sách tóm tắt các phản ánh</returns>
        Task<ApiResponse<object>> GetWeeklyStudentReportsAsync();

        /// <summary>
        /// Lấy tóm tắt các phản ánh trong tuần của học sinh cụ thể
        /// </summary>
        /// <param name="studentId">ID của học sinh</param>
        /// <returns>Tóm tắt các phản ánh của học sinh</returns>
        Task<ApiResponse<object>> GetWeeklyStudentReportAsync(string studentId);
    }
}
