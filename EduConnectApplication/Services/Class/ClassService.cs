using EduConnect.Application.DTOs.Response;
using EduConnect.Application.DTOs.Request;
using EduConnect.Application.Services.Interface;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduConnect.Application.Services.Class
{
    public class ClassService : IClassService
    {
        private readonly IBaseRepository<EduConnect.Domain.Entities.Class, int> _classRepository;
        private readonly IBaseRepository<Student, string> _studentRepository;
        private readonly IBaseRepository<Parent, string> _parentRepository;

        public ClassService(
            IBaseRepository<EduConnect.Domain.Entities.Class, int> classRepository,
            IBaseRepository<Student, string> studentRepository,
            IBaseRepository<Parent, string> parentRepository)
        {
            _classRepository = classRepository;
            _studentRepository = studentRepository;
            _parentRepository = parentRepository;
        }

        public async Task<IEnumerable<EduConnect.Domain.Entities.Class>> GetAllClassesAsync()
        {
            return await _classRepository.GetAllAsync();
        }

        public async Task<IEnumerable<EduConnect.Domain.Entities.Class>> SearchClassesAsync(string? searchTerm)
        {
            var all = await _classRepository.GetAllAsync();
            if (string.IsNullOrWhiteSpace(searchTerm)) return all;
            return all.Where(c => c.ClassName != null && c.ClassName.ToLower().Contains(searchTerm.ToLower()));
        }

        public async Task<IEnumerable<EduConnect.Domain.Entities.Class>> SortClassesAsync(string? sortBy, bool ascending = true)
        {
            var all = await _classRepository.GetAllAsync();
            if (string.IsNullOrWhiteSpace(sortBy) || sortBy.ToLower() == "classname")
            {
                return ascending ? all.OrderBy(c => c.ClassName) : all.OrderByDescending(c => c.ClassName);
            }
            return all;
        }

        public async Task<PagedResponse<EduConnect.Domain.Entities.Class>> GetPagedClassesAsync(int pageIndex, int pageSize, string? searchTerm = null, string? sortBy = null, bool ascending = true)
        {
            var all = await _classRepository.GetAllAsync();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                all = all.Where(c => c.ClassName != null && c.ClassName.ToLower().Contains(searchTerm.ToLower()));
            }
            if (string.IsNullOrWhiteSpace(sortBy) || sortBy.ToLower() == "classname")
            {
                all = ascending ? all.OrderBy(c => c.ClassName) : all.OrderByDescending(c => c.ClassName);
            }
            var totalCount = all.Count();
            var items = all.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return new PagedResponse<EduConnect.Domain.Entities.Class>
            {
                Items = items,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }

        // CRUD học sinh vào lớp (chỉ Principal)
        public async Task<ApiResponse<List<ClassStudentResponse>>> GetStudentsInClassAsync(int classId)
        {
            try
            {
                var classEntity = await _classRepository.GetByIdAsync(classId);
                if (classEntity == null)
                {
                    return new ApiResponse<List<ClassStudentResponse>>
                    {
                        Success = false,
                        Message = "Không tìm thấy lớp học"
                    };
                }

                var students = await _studentRepository.GetAllAsync();
                var studentsInClass = students.Where(s => s.ClassID == classId && s.IsDeleted != true).ToList();

                var result = new List<ClassStudentResponse>();
                foreach (var student in studentsInClass)
                {
                    var parent = await _parentRepository.GetByIdAsync(student.ParentID);
                    result.Add(new ClassStudentResponse
                    {
                        StudentID = student.StudentID,
                        FullName = student.FullName ?? "",
                        DateOfBirth = student.DateOfBirth,
                        Gender = student.Gender,
                        ParentID = student.ParentID,
                        ParentName = parent?.FullName ?? "",
                        ClassID = student.ClassID,
                        ClassName = classEntity.ClassName
                    });
                }

                return new ApiResponse<List<ClassStudentResponse>>
                {
                    Success = true,
                    Data = result,
                    Message = "Lấy danh sách học sinh trong lớp thành công"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<ClassStudentResponse>>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<string>> AddStudentsToClassAsync(AddStudentsToClassRequest request)
        {
            try
            {
                var classEntity = await _classRepository.GetByIdAsync(request.ClassID);
                if (classEntity == null)
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Không tìm thấy lớp học"
                    };
                }

                var students = await _studentRepository.GetAllAsync();
                var studentsToUpdate = students.Where(s => request.StudentIDs.Contains(s.StudentID) && s.IsDeleted != true).ToList();

                if (!studentsToUpdate.Any())
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Không tìm thấy học sinh nào để thêm"
                    };
                }

                foreach (var student in studentsToUpdate)
                {
                    student.ClassID = request.ClassID;
                    await _studentRepository.UpdateAsync(student.StudentID, student);
                }

                return new ApiResponse<string>
                {
                    Success = true,
                    Message = $"Đã thêm {studentsToUpdate.Count} học sinh vào lớp {classEntity.ClassName}"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<string>> RemoveStudentsFromClassAsync(RemoveStudentsFromClassRequest request)
        {
            try
            {
                var classEntity = await _classRepository.GetByIdAsync(request.ClassID);
                if (classEntity == null)
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Không tìm thấy lớp học"
                    };
                }

                var students = await _studentRepository.GetAllAsync();
                var studentsToUpdate = students.Where(s => request.StudentIDs.Contains(s.StudentID) && s.ClassID == request.ClassID && s.IsDeleted != true).ToList();

                if (!studentsToUpdate.Any())
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Không tìm thấy học sinh nào để xóa khỏi lớp"
                    };
                }

                foreach (var student in studentsToUpdate)
                {
                    student.ClassID = null;
                    await _studentRepository.UpdateAsync(student.StudentID, student);
                }

                return new ApiResponse<string>
                {
                    Success = true,
                    Message = $"Đã xóa {studentsToUpdate.Count} học sinh khỏi lớp {classEntity.ClassName}"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<string>> UpdateStudentClassAsync(UpdateStudentClassRequest request)
        {
            try
            {
                var student = await _studentRepository.GetByIdAsync(request.StudentID);
                if (student == null || student.IsDeleted == true)
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Không tìm thấy học sinh"
                    };
                }

                if (request.NewClassID.HasValue)
                {
                    var newClass = await _classRepository.GetByIdAsync(request.NewClassID.Value);
                    if (newClass == null)
                    {
                        return new ApiResponse<string>
                        {
                            Success = false,
                            Message = "Không tìm thấy lớp học mới"
                        };
                    }
                }

                student.ClassID = request.NewClassID;
                await _studentRepository.UpdateAsync(student.StudentID, student);

                var message = request.NewClassID.HasValue 
                    ? $"Đã chuyển học sinh {student.FullName} sang lớp mới"
                    : $"Đã xóa học sinh {student.FullName} khỏi lớp";

                return new ApiResponse<string>
                {
                    Success = true,
                    Message = message
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<List<ClassStudentResponse>>> GetStudentsNotInClassAsync()
        {
            try
            {
                var students = await _studentRepository.GetAllAsync();
                var studentsNotInClass = students.Where(s => s.ClassID == null && s.IsDeleted != true).ToList();

                var result = new List<ClassStudentResponse>();
                foreach (var student in studentsNotInClass)
                {
                    var parent = await _parentRepository.GetByIdAsync(student.ParentID);
                    result.Add(new ClassStudentResponse
                    {
                        StudentID = student.StudentID,
                        FullName = student.FullName ?? "",
                        DateOfBirth = student.DateOfBirth,
                        Gender = student.Gender,
                        ParentID = student.ParentID,
                        ParentName = parent?.FullName ?? "",
                        ClassID = null,
                        ClassName = null
                    });
                }

                return new ApiResponse<List<ClassStudentResponse>>
                {
                    Success = true,
                    Data = result,
                    Message = "Lấy danh sách học sinh chưa phân lớp thành công"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<ClassStudentResponse>>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }
    }
}
