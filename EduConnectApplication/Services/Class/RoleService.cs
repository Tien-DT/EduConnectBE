using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using EduConnect.Application.Services.Interface;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Repositories.Interface;
using EduConnect.Infrastruture.UnitOfWork.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace EduConnect.Application.Services.Class
{
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStudentRepository _studentRepository;
        private readonly ITeacherRepository _teacherRepository;
        private readonly IParentRepository _parentRepository;
        private readonly ILogger<RoleService> _logger;

        public RoleService(
            IUnitOfWork unitOfWork,
            IStudentRepository studentRepository,
            ITeacherRepository teacherRepository,
            IParentRepository parentRepository,
            ILogger<RoleService> logger)
        {
            _unitOfWork = unitOfWork;
            _studentRepository = studentRepository;
            _teacherRepository = teacherRepository;
            _parentRepository = parentRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<AssignRoleResponse>> AssignStudentRoleAsync(AssignStudentRoleListRequest request)
        {
            var response = new AssignRoleResponse();
            var results = new List<AssignRoleResult>();

            foreach (var studentRequest in request.Students)
            {
                var result = new AssignStudentRoleResult
                {
                    AccountID = studentRequest.AccountID,
                    FullName = studentRequest.FullName,
                    ID = studentRequest.StudentID,
                    RoleAssigned = "Student",
                    ParentID = studentRequest.ParentID
                };

                try
                {
                    var user = await _unitOfWork.AuthRepository.GetUserByIdAsync(studentRequest.AccountID);
                    if (user == null)
                    {
                        result.Success = false;
                        result.ErrorMessage = "User not found";
                        results.Add(result);
                        continue;
                    }
                    var existingStudent = (await _studentRepository.GetAllAsync())
                        .FirstOrDefault(s => s.StudentID == studentRequest.StudentID);
                    
                    if (existingStudent != null)
                    {
                        result.Success = false;
                        result.ErrorMessage = "Student ID already exists";
                        results.Add(result);
                        continue;
                    }

                    var currentRoles = await _unitOfWork.AuthRepository.GetRolesAsync(user);
                    var defaultRole = currentRoles.FirstOrDefault(); 
                    
                    IdentityResult roleResult;
                    if (!string.IsNullOrEmpty(defaultRole))
                    {
                        roleResult = await _unitOfWork.AuthRepository.ReplaceRoleAsync(user, defaultRole, "Student");
                    }
                    else
                    {
                        roleResult = await _unitOfWork.AuthRepository.AddToRoleByNameAsync(user, "Student");
                    }
                    if (!roleResult.Succeeded)
                    {
                        result.Success = false;
                        result.ErrorMessage = "Failed to assign role: " + string.Join(", ", roleResult.Errors.Select(e => e.Description));
                        results.Add(result);
                        continue;
                    }

                    var student = new Student
                    {
                        StudentID = studentRequest.StudentID,
                        AccountID = studentRequest.AccountID,
                        ParentID = studentRequest.ParentID,
                        FullName = studentRequest.FullName,
                        IsDeleted = false
                    };

                    await _studentRepository.CreateAsync(student);
                    await _unitOfWork.SaveChangesAsync();

                    result.Success = true;
                    results.Add(result);
                    _logger.LogInformation($"Successfully assigned Student role to {studentRequest.AccountID}");
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.ErrorMessage = ex.Message;
                    results.Add(result);
                    _logger.LogError(ex, $"Error assigning Student role to {studentRequest.AccountID}");
                }
            }

            response.Results = results;
            response.TotalProcessed = results.Count;
            response.SuccessCount = results.Count(r => r.Success);
            response.FailureCount = results.Count(r => !r.Success);

            return ApiResponse<AssignRoleResponse>.SuccessResponse(response, 
                $"Processed {response.TotalProcessed} student role assignments. Success: {response.SuccessCount}, Failed: {response.FailureCount}");
        }

        public async Task<ApiResponse<AssignRoleResponse>> AssignTeacherRoleAsync(AssignTeacherRoleListRequest request)
        {
            var response = new AssignRoleResponse();
            var results = new List<AssignRoleResult>();

            foreach (var teacherRequest in request.Teachers)
            {
                var result = new AssignRoleResult
                {
                    AccountID = teacherRequest.AccountID,
                    FullName = teacherRequest.FullName,
                    ID = teacherRequest.TeacherID,
                    RoleAssigned = "Teacher"
                };

                try
                {
                    var user = await _unitOfWork.AuthRepository.GetUserByIdAsync(teacherRequest.AccountID);
                    if (user == null)
                    {
                        result.Success = false;
                        result.ErrorMessage = "User not found";
                        results.Add(result);
                        continue;
                    }
                    var existingTeacher = (await _teacherRepository.GetAllAsync())
                        .FirstOrDefault(t => t.TeacherID == teacherRequest.TeacherID);
                    
                    if (existingTeacher != null)
                    {
                        result.Success = false;
                        result.ErrorMessage = "Teacher ID already exists";
                        results.Add(result);
                        continue;
                    }

                    var currentRoles = await _unitOfWork.AuthRepository.GetRolesAsync(user);
                    var defaultRole = currentRoles.FirstOrDefault(); 
                    
                    IdentityResult roleResult;
                    if (!string.IsNullOrEmpty(defaultRole))
                    {
                        roleResult = await _unitOfWork.AuthRepository.ReplaceRoleAsync(user, defaultRole, "Teacher");
                    }
                    else
                    {
                        roleResult = await _unitOfWork.AuthRepository.AddToRoleByNameAsync(user, "Teacher");
                    }
                    if (!roleResult.Succeeded)
                    {
                        result.Success = false;
                        result.ErrorMessage = "Failed to assign role: " + string.Join(", ", roleResult.Errors.Select(e => e.Description));
                        results.Add(result);
                        continue;
                    }
                    var teacher = new Teacher
                    {
                        TeacherID = teacherRequest.TeacherID,
                        AccountID = teacherRequest.AccountID,
                        FullName = teacherRequest.FullName,
                        IsDeleted = false
                    };

                    await _teacherRepository.CreateAsync(teacher);
                    await _unitOfWork.SaveChangesAsync();

                    result.Success = true;
                    results.Add(result);
                    _logger.LogInformation($"Successfully assigned Teacher role to {teacherRequest.AccountID}");
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.ErrorMessage = ex.Message;
                    results.Add(result);
                    _logger.LogError(ex, $"Error assigning Teacher role to {teacherRequest.AccountID}");
                }
            }

            response.Results = results;
            response.TotalProcessed = results.Count;
            response.SuccessCount = results.Count(r => r.Success);
            response.FailureCount = results.Count(r => !r.Success);

            return ApiResponse<AssignRoleResponse>.SuccessResponse(response, 
                $"Processed {response.TotalProcessed} teacher role assignments. Success: {response.SuccessCount}, Failed: {response.FailureCount}");
        }

        public async Task<ApiResponse<AssignRoleResponse>> AssignParentRoleAsync(AssignParentRoleListRequest request)
        {
            var response = new AssignRoleResponse();
            var results = new List<AssignRoleResult>();

            foreach (var parentRequest in request.Parents)
            {
                var result = new AssignRoleResult
                {
                    AccountID = parentRequest.AccountID,
                    FullName = parentRequest.FullName,
                    ID = parentRequest.ParentID,
                    RoleAssigned = "Parent"
                };

                try
                {
                    var user = await _unitOfWork.AuthRepository.GetUserByIdAsync(parentRequest.AccountID);
                    if (user == null)
                    {
                        result.Success = false;
                        result.ErrorMessage = "User not found";
                        results.Add(result);
                        continue;
                    }
                    var existingParent = (await _parentRepository.GetAllAsync())
                        .FirstOrDefault(p => p.ParentID == parentRequest.ParentID);
                    
                    if (existingParent != null)
                    {
                        result.Success = false;
                        result.ErrorMessage = "Parent ID already exists";
                        results.Add(result);
                        continue;
                    }
                    var currentRoles = await _unitOfWork.AuthRepository.GetRolesAsync(user);
                    var defaultRole = currentRoles.FirstOrDefault(); 
                    IdentityResult roleResult;
                    if (!string.IsNullOrEmpty(defaultRole))
                    {
                        roleResult = await _unitOfWork.AuthRepository.ReplaceRoleAsync(user, defaultRole, "Parent");
                    }
                    else
                    {
                        roleResult = await _unitOfWork.AuthRepository.AddToRoleByNameAsync(user, "Parent");
                    }
                    if (!roleResult.Succeeded)
                    {
                        result.Success = false;
                        result.ErrorMessage = "Failed to assign role: " + string.Join(", ", roleResult.Errors.Select(e => e.Description));
                        results.Add(result);
                        continue;
                    }

                    var parent = new Parent
                    {
                        ParentID = parentRequest.ParentID,
                        AccountID = parentRequest.AccountID,
                        FullName = parentRequest.FullName,
                        IsDeleted = false
                    };

                    await _parentRepository.CreateAsync(parent);
                    await _unitOfWork.SaveChangesAsync();

                    result.Success = true;
                    results.Add(result);
                    _logger.LogInformation($"Successfully assigned Parent role to {parentRequest.AccountID}");
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.ErrorMessage = ex.Message;
                    results.Add(result);
                    _logger.LogError(ex, $"Error assigning Parent role to {parentRequest.AccountID}");
                }
            }

            response.Results = results;
            response.TotalProcessed = results.Count;
            response.SuccessCount = results.Count(r => r.Success);
            response.FailureCount = results.Count(r => !r.Success);

            return ApiResponse<AssignRoleResponse>.SuccessResponse(response, 
                $"Processed {response.TotalProcessed} parent role assignments. Success: {response.SuccessCount}, Failed: {response.FailureCount}");
        }
    }
}
