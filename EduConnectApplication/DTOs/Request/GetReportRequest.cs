namespace EduConnectApplication.DTOs.Request
{
    public class GetReportRequest
    {
        public string? AccountId { get; set; }
        public string? ReportType { get; set; }
        public string? Status { get; set; }
        public string? SortBy { get; set; } = "CreatedDate";
        public bool SortDesc { get; set; } = true;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
    }
}
