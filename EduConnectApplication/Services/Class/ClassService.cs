using AutoMapper;
using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using EduConnect.Application.QueryHelpers;
using EduConnect.Application.Services.Interface;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Repositories.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClassEntity = EduConnect.Domain.Entities.Class;

namespace EduConnect.Application.Services.Class
{
    public class ClassService : IClassService
    {
        private readonly IClassRepository _classRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IMapper _mapper;
        
        public ClassService(
            IClassRepository classRepository,
            IStudentRepository studentRepository,
            IMapper mapper)
        {
            _classRepository = classRepository;
            _studentRepository = studentRepository;
            _mapper = mapper;
        }
        public async Task<IEnumerable<ClassEntity>> GetAllClassesAsync()
        {
            return await _classRepository.GetAllAsync();
        }
        public async Task<IEnumerable<ClassEntity>> SearchClassesAsync(string? searchTerm)
        {
            var query = (await _classRepository.GetAllAsync()).AsQueryable();
            query = ClassQueryHelper.ApplySearch(query, searchTerm);
            return query.ToList();
        }
        
        public async Task<IEnumerable<ClassEntity>> SortClassesAsync(string? sortBy, bool ascending = true)
        {
            var query = (await _classRepository.GetAllAsync()).AsQueryable();
            query = ClassQueryHelper.ApplySort(query, sortBy, ascending);
            return query.ToList();
        }
        
        public async Task<PagedResponse<ClassEntity>> GetPagedClassesAsync(int pageIndex, int pageSize, string? searchTerm = null, string? sortBy = null, bool ascending = true)
        {
            var query = (await _classRepository.GetAllAsync()).AsQueryable();
            
            query = ClassQueryHelper.ApplySearch(query, searchTerm);
            query = ClassQueryHelper.ApplySort(query, sortBy, ascending);
            
            var totalCount = query.Count();
            var items = query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            
            return new PagedResponse<ClassEntity>
            {
                Items = items,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<List<ClassStudentResponse>>> GetStudentsInClassAsync(int classId)
        {
            try
            {
                var students = await _studentRepository.GetStudentsByClassIdAsync(classId);
                var response = _mapper.Map<List<ClassStudentResponse>>(students);
                return new ApiResponse<List<ClassStudentResponse>>
                {
                    Success = true,
                    Data = response,
                    Message = "Students retrieved successfully"
                };
            }
            catch (System.Exception ex)
            {
                return new ApiResponse<List<ClassStudentResponse>>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<List<ClassStudentResponse>>> GetStudentsNotInClassAsync()
        {
            try
            {
                var students = await _studentRepository.GetStudentsWithoutClassAsync();
                var response = _mapper.Map<List<ClassStudentResponse>>(students);
                return new ApiResponse<List<ClassStudentResponse>>
                {
                    Success = true,
                    Data = response,
                    Message = "Unassigned students retrieved successfully"
                };
            }
            catch (System.Exception ex)
            {
                return new ApiResponse<List<ClassStudentResponse>>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<string>> AddStudentsToClassAsync(int classId, List<string> studentIds)
        {
            try
            {
                var result = await _studentRepository.AddStudentsToClassAsync(classId, studentIds);
                if (result)
                {
                    return new ApiResponse<string>
                    {
                        Success = true,
                        Data = "Students added to class successfully",
                        Message = "Operation completed"
                    };
                }
                else
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Failed to add students to class"
                    };
                }
            }
            catch (System.Exception ex)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<string>> RemoveStudentsFromClassAsync(int classId, List<string> studentIds)
        {
            try
            {
                var result = await _studentRepository.RemoveStudentsFromClassAsync(studentIds);
                if (result)
                {
                    return new ApiResponse<string>
                    {
                        Success = true,
                        Data = "Students removed from class successfully",
                        Message = "Operation completed"
                    };
                }
                else
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Failed to remove students from class"
                    };
                }
            }
            catch (System.Exception ex)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<string>> UpdateStudentClassAsync(string studentId, int? newClassId)
        {
            try
            {
                var result = await _studentRepository.UpdateStudentClassAsync(studentId, newClassId);
                if (result)
                {
                    return new ApiResponse<string>
                    {
                        Success = true,
                        Data = "Student class updated successfully",
                        Message = "Operation completed"
                    };
                }
                else
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Failed to update student class"
                    };
                }
            }
            catch (System.Exception ex)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }        public async Task<ApiResponse<PagedResponse<ClassResponse>>> GetClassesAsync(int pageIndex, int pageSize, string? searchTerm = null, string sortBy = "ClassName", bool ascending = true)
        {
            try
            {
                var query = (await _classRepository.GetAllActiveClassesAsync()).AsQueryable();
                
                // Apply search
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = ClassQueryHelper.ApplySearch(query, searchTerm);
                }
                
                // Apply sorting
                query = ClassQueryHelper.ApplySort(query, sortBy, ascending);
                
                var totalCount = query.Count();
                
                // Apply paging
                var items = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
                
                // Map to ClassResponse - chỉ lấy field từ bảng Class
                var classResponses = items.Select(c => new ClassResponse
                {
                    ClassId = c.ClassID,
                    ClassName = c.ClassName,
                    IsDeleted = c.IsDeleted
                }).ToList();
                
                var pagedResponse = new PagedResponse<ClassResponse>
                {
                    Items = classResponses,
                    TotalCount = totalCount,
                    PageIndex = pageIndex,
                    PageSize = pageSize
                };
                
                return ApiResponse<PagedResponse<ClassResponse>>.SuccessResponse(pagedResponse, "Classes retrieved successfully");
            }
            catch (System.Exception ex)
            {
                return ApiResponse<PagedResponse<ClassResponse>>.ErrorResponse(new List<string> { ex.Message }, "Failed to retrieve classes");
            }
        }
        
        public async Task<ApiResponse<ClassResponse>> GetClassByIdAsync(int classId)
        {
            try
            {
                var classEntity = await _classRepository.GetActiveClassByIdAsync(classId);
                
                if (classEntity == null)
                {
                    return ApiResponse<ClassResponse>.ErrorResponse(new List<string> { "Class not found" }, "Class not found");
                }
                
                var classResponse = new ClassResponse
                {
                    ClassId = classEntity.ClassID,
                    ClassName = classEntity.ClassName,
                    IsDeleted = classEntity.IsDeleted
                };
                
                return ApiResponse<ClassResponse>.SuccessResponse(classResponse, "Class retrieved successfully");
            }
            catch (System.Exception ex)
            {
                return ApiResponse<ClassResponse>.ErrorResponse(new List<string> { ex.Message }, "Failed to retrieve class");
            }
        }
    }
}
