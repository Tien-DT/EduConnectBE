namespace EduConnect.Infrastruture.Options
{
    public class FirebaseOption
    {
        public string Url { get; set; } = string.Empty;
        public string ServerKey { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public string? ServiceAccountKeyPath { get; set; }
        public string? ServiceAccountJson { get; set; }
    }
}
