using AutoMapper;
using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using EduConnect.Domain.Entities;

namespace EduConnect.Application.Mapping
{
    public class ApplicationProfile : Profile
    {
        public ApplicationProfile()
        {
            CreateMap<RegisterRequest, ApplicationUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.NormalizedEmail, opt => opt.MapFrom(src => src.Email != null ? src.Email.ToUpperInvariant() : null))
                .ForMember(dest => dest.NormalizedUserName, opt => opt.MapFrom(src => src.Email != null ? src.Email.ToUpperInvariant() : null))
                .ForMember(dest => dest.EmailConfirmed, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.Ignore())
                .ForMember(dest => dest.TwoFactorEnabled, opt => opt.Ignore())
                .ForMember(dest => dest.LockoutEnabled, opt => opt.Ignore())
                .ForMember(dest => dest.AccessFailedCount, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
                .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore())
                .ForMember(dest => dest.AvatarUrl, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Student, opt => opt.Ignore())
                .ForMember(dest => dest.Teacher, opt => opt.Ignore())
                .ForMember(dest => dest.Parent, opt => opt.Ignore())
                .ForMember(dest => dest.Principal, opt => opt.Ignore())
                .ForMember(dest => dest.RefreshToken, opt => opt.Ignore());

            CreateMap<ApplicationUser, RegisterResponse>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName));
    
            CreateMap<Exam, ExamResponse>()
                .ForMember(dest => dest.SubjectName, opt => opt.MapFrom(src => src.Subject != null ? src.Subject.SubjectName : string.Empty))
                .ForMember(dest => dest.SubjectCode, opt => opt.MapFrom(src => src.Subject != null ? src.Subject.SubjectCode : string.Empty))
                .ForMember(dest => dest.ExamNote, opt => opt.MapFrom(src => src.ExamNote ?? string.Empty))
                .ForMember(dest => dest.Room, opt => opt.MapFrom(src => src.Room ?? string.Empty))
                .ForMember(dest => dest.StudentId, opt => opt.MapFrom(src => src.StudentID ?? string.Empty))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate));

            CreateMap<Student, ClassStudentResponse>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName ?? string.Empty))
                .ForMember(dest => dest.ParentId, opt => opt.MapFrom(src => src.ParentID))
                .ForMember(dest => dest.ParentName, opt => opt.MapFrom(src => src.Parent != null ? src.Parent.FullName : string.Empty))
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.Class != null ? src.Class.ClassName : string.Empty));

            CreateMap<LessonReport, LessonReportResponse>()
                .ForMember(dest => dest.LessonName, opt => opt.MapFrom(src => src.Lesson != null ? $"Lesson {src.Lesson.LessonID}" : null))
                .ForMember(dest => dest.SubjectName, opt => opt.MapFrom(src => src.TimetableSlot.Subject.SubjectName))
                .ForMember(dest => dest.SubjectCode, opt => opt.MapFrom(src => src.TimetableSlot.Subject.SubjectCode))
                .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src => src.TimetableSlot.Teacher.FullName))
                .ForMember(dest => dest.TeacherId, opt => opt.MapFrom(src => src.TimetableSlot.TeacherID))
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.TimetableSlot.Timetable.Class.ClassName))
                .ForMember(dest => dest.ClassId, opt => opt.MapFrom(src => src.TimetableSlot.Timetable.ClassID))
                .ForMember(dest => dest.DayOfWeek, opt => opt.MapFrom(src => src.TimetableSlot.DayOfWeek))
                .ForMember(dest => dest.Period, opt => opt.MapFrom(src => src.TimetableSlot.Period))
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.TimetableSlot.StartTime))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.TimetableSlot.EndTime))
                .ForMember(dest => dest.Room, opt => opt.MapFrom(src => src.TimetableSlot.Room))
                .ForMember(dest => dest.StudentNotes, opt => opt.MapFrom(src => src.StudentLessonNotes));

            CreateMap<StudentLessonNote, StudentLessonNoteResponse>()
                .ForMember(dest => dest.NoteId, opt => opt.MapFrom(src => src.StudentLessonNoteID))
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student.FullName))
                .ForMember(dest => dest.NoteContent, opt => opt.MapFrom(src => src.Description));

            CreateMap<CreateLessonReportRequest, LessonReport>()
                .ForMember(dest => dest.ReportID, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.TimetableSlot, opt => opt.Ignore())
                .ForMember(dest => dest.Lesson, opt => opt.Ignore())
                .ForMember(dest => dest.StudentLessonNotes, opt => opt.Ignore());

            CreateMap<CreateStudentLessonNoteRequest, StudentLessonNote>()
                .ForMember(dest => dest.StudentLessonNoteID, opt => opt.Ignore())
                .ForMember(dest => dest.LessonReportID, opt => opt.Ignore())
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.NoteContent))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Points, opt => opt.Ignore())
                .ForMember(dest => dest.LessonReport, opt => opt.Ignore())
                .ForMember(dest => dest.Student, opt => opt.Ignore());

            CreateMap<Attendance, AttendanceResponse>();
            CreateMap<AttendanceRequest, Attendance>()
                .ForMember(dest => dest.AttendanceID, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.TimetableSlot, opt => opt.Ignore())
                .ForMember(dest => dest.Student, opt => opt.Ignore());

            // Subject mappings
            CreateMap<Subject, SubjectResponse>()
                .ForMember(dest => dest.LessonCount, opt => opt.MapFrom(src => src.Lessons.Count))
                .ForMember(dest => dest.ExamCount, opt => opt.MapFrom(src => src.Exams.Count));

            CreateMap<CreateSubjectRequest, Subject>()
                .ForMember(dest => dest.SubjectId, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Lessons, opt => opt.Ignore())
                .ForMember(dest => dest.Exams, opt => opt.Ignore());

            CreateMap<UpdateSubjectRequest, Subject>()
                .ForMember(dest => dest.SubjectId, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Lessons, opt => opt.Ignore())
                .ForMember(dest => dest.Exams, opt => opt.Ignore());

            // Exam mappings
            CreateMap<UpdateExamRequest, Exam>()
                .ForMember(dest => dest.ExamId, opt => opt.Ignore())
                .ForMember(dest => dest.StudentID, opt => opt.MapFrom(src => src.StudentId))
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Subject, opt => opt.Ignore())
                .ForMember(dest => dest.Student, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUser, opt => opt.Ignore());
        }
    }
}
