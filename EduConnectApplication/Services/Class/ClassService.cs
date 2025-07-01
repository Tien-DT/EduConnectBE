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

namespace EduConnect.Application.Services.Class
{
    public class ClassService : IClassService
    {
        private readonly IBaseRepository<EduConnect.Domain.Entities.Class, int> _classRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IMapper _mapper;
        
        public ClassService(
            IBaseRepository<EduConnect.Domain.Entities.Class, int> classRepository,
            IStudentRepository studentRepository,
            IMapper mapper)
        {
            _classRepository = classRepository;
            _studentRepository = studentRepository;
            _mapper = mapper;
        }
        public async Task<IEnumerable<EduConnect.Domain.Entities.Class>> GetAllClassesAsync()
        {
            return await _classRepository.GetAllAsync();
        }
        public async Task<IEnumerable<EduConnect.Domain.Entities.Class>> SearchClassesAsync(string? searchTerm)
        {
            var query = (await _classRepository.GetAllAsync()).AsQueryable();
            query = ClassQueryHelper.ApplySearch(query, searchTerm);
            return query.ToList();
        }
        
        public async Task<IEnumerable<EduConnect.Domain.Entities.Class>> SortClassesAsync(string? sortBy, bool ascending = true)
        {
            var query = (await _classRepository.GetAllAsync()).AsQueryable();
            query = ClassQueryHelper.ApplySort(query, sortBy, ascending);
            return query.ToList();
        }
        
        public async Task<PagedResponse<EduConnect.Domain.Entities.Class>> GetPagedClassesAsync(int pageIndex, int pageSize, string? searchTerm = null, string? sortBy = null, bool ascending = true)
        {
            var query = (await _classRepository.GetAllAsync()).AsQueryable();
            
            query = ClassQueryHelper.ApplySearch(query, searchTerm);
            query = ClassQueryHelper.ApplySort(query, sortBy, ascending);
            
            var totalCount = query.Count();
            var items = query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            
            return new PagedResponse<EduConnect.Domain.Entities.Class>
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

        public async Task<ApiResponse<string>> AddStudentsToClassAsync(AddStudentsToClassRequest request)
        {
            try
            {
                var result = await _studentRepository.AddStudentsToClassAsync(request.ClassID, request.StudentIDs);
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

        public async Task<ApiResponse<string>> RemoveStudentsFromClassAsync(RemoveStudentsFromClassRequest request)
        {
            try
            {
                var result = await _studentRepository.RemoveStudentsFromClassAsync(request.StudentIDs);
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

        public async Task<ApiResponse<string>> UpdateStudentClassAsync(UpdateStudentClassRequest request)
        {
            try
            {
                var result = await _studentRepository.UpdateStudentClassAsync(request.StudentID, request.NewClassID);
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
        }
    }
}
