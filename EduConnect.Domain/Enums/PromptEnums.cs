namespace EduConnect.Domain.Enums
{
    public static class PromptType
    {
        public const string LESSON_PLAN = "LESSON_PLAN";
        public const string STUDENT_EVALUATION = "STUDENT_EVALUATION";
        public const string PARENT_COMMUNICATION = "PARENT_COMMUNICATION";
        public const string REPORT_GENERATION = "REPORT_GENERATION";
        public const string FEEDBACK_TEMPLATE = "FEEDBACK_TEMPLATE";
        public const string NOTIFICATION_TEMPLATE = "NOTIFICATION_TEMPLATE";
        public const string GENERAL = "GENERAL";
        public const string CUSTOM = "CUSTOM";
    }

    public static class PromptStatus
    {
        public const string DRAFT = "DRAFT";
        public const string ACTIVE = "ACTIVE";
        public const string INACTIVE = "INACTIVE";
        public const string ARCHIVED = "ARCHIVED";
        public const string DELETED = "DELETED";
    }
}
