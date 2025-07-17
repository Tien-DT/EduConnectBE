using AutoMapper;
using EduConnect.Application.DTOs.Response;
using EduConnect.Application.QueryHelpers;
using EduConnect.Application.Services.Interface;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduConnect.Application.Services.Class
{
    public class ParentService : IParentService
    {
        private readonly IParentRepository _parentRepository;

        public ParentService(IParentRepository parentRepository)
        {
            _parentRepository = parentRepository;
        }

        public async Task<ApiResponse<ParentResponse>> GetParentByIdAsync(string parentId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(parentId))
                {
                    return ApiResponse<ParentResponse>.ErrorResponse(
                        new List<string> { "Parent ID is required" }, 
                        "Invalid parent ID");
                }

                var parent = await _parentRepository.GetParentWithDetailsAsync(parentId);
                
                if (parent == null)
                {
                    return ApiResponse<ParentResponse>.ErrorResponse(
                        new List<string> { "Parent not found" }, 
                        "Parent not found");
                }

                var parentResponse = MapToParentResponse(parent);

                return ApiResponse<ParentResponse>.SuccessResponse(
                    parentResponse, 
                    "Parent retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<ParentResponse>.ErrorResponse(
                    new List<string> { ex.Message }, 
                    "Error retrieving parent");
            }
        }

        public async Task<PagedApiResponse<List<ParentResponse>>> GetParentsAsync(
            int pageIndex = 0, 
            int pageSize = 10, 
            string? searchTerm = null, 
            string sortBy = "FullName", 
            bool ascending = true)
        {
            return await FilterSortSearchParentsAsync(searchTerm, sortBy, ascending, pageIndex, pageSize);
        }

        private ParentResponse MapToParentResponse(Parent parent)
        {
            return new ParentResponse
            {
                ParentId = parent.ParentID,
                AccountId = parent.AccountID,
                FullName = parent.FullName ?? string.Empty,
                Email = parent.Account?.Email ?? string.Empty,
                FirstName = parent.Account?.FirstName ?? string.Empty,
                LastName = parent.Account?.LastName ?? string.Empty,
                AvatarUrl = parent.Account?.AvatarUrl ?? string.Empty,
                IsActive = parent.Account?.IsActive ?? false,
                CreatedAt = parent.Account?.CreatedAt,
                UpdatedAt = parent.Account?.UpdatedAt,
                StudentNames = parent.Students?.Where(s => s.IsDeleted != true)
                    .Select(s => s.FullName ?? string.Empty)
                    .Where(name => !string.IsNullOrEmpty(name))
                    .ToList() ?? new List<string>(),
                TotalStudents = parent.Students?.Count(s => s.IsDeleted != true) ?? 0
            };
        }

        public async Task<PagedApiResponse<List<ParentResponse>>> FilterSortSearchParentsAsync(
            string? searchTerm = null,
            string? sortBy = null,
            bool ascending = true,
            int pageIndex = 0,
            int pageSize = 10)
        {
            try
            {
                if (pageSize <= 0) pageSize = 10;
                if (pageIndex < 0) pageIndex = 0;

                var query = _parentRepository.GetParentQueryable();

                query = ParentQueryHelper.ApplySearch(query, searchTerm);
                query = ParentQueryHelper.ApplySort(query, sortBy, ascending);

                var totalCount = await Task.FromResult(query.Count());
                var items = await Task.FromResult(query.Skip(pageIndex * pageSize).Take(pageSize).ToList());

                var parentResponses = items.Select(MapToParentResponse).ToList();

                return PagedApiResponse<List<ParentResponse>>.SuccessPagedResponse(
                    parentResponses,
                    "Parents filtered, sorted, and searched successfully",
                    totalCount,
                    pageIndex,
                    pageSize
                );
            }
            catch (Exception ex)
            {
                return PagedApiResponse<List<ParentResponse>>.ErrorPagedResponse(
                    new List<string> { ex.Message },
                    "Error filtering, sorting, and searching parents",
                    pageIndex,
                    pageSize
                );
            }
        }

        public async Task<PagedApiResponse<List<StudentResponse>>> GetStudentsByParentIdAsync(
            string parentId,
            int pageIndex = 0,
            int pageSize = 10,
            string? searchTerm = null,
            string sortBy = "FullName",
            bool ascending = true)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(parentId))
                {
                    return PagedApiResponse<List<StudentResponse>>.ErrorPagedResponse(
                        new List<string> { "Parent ID is required" },
                        "Invalid parent ID",
                        pageIndex,
                        pageSize);
                }

                if (pageSize <= 0) pageSize = 10;
                if (pageIndex < 0) pageIndex = 0;

                var parent = await _parentRepository.GetParentWithDetailsAsync(parentId);
                
                if (parent == null)
                {
                    return PagedApiResponse<List<StudentResponse>>.ErrorPagedResponse(
                        new List<string> { "Parent not found" },
                        "Parent not found",
                        pageIndex,
                        pageSize);
                }

                var students = parent.Students?.Where(s => s.IsDeleted != true).AsQueryable();
                
                if (students == null || !students.Any())
                {
                    return PagedApiResponse<List<StudentResponse>>.SuccessPagedResponse(
                        new List<StudentResponse>(),
                        "No students found for this parent",
                        0,
                        pageIndex,
                        pageSize);
                }
                students = StudentQueryHelper.ApplySearch(students, searchTerm);
                students = StudentQueryHelper.ApplySort(students, sortBy, ascending);

                var totalCount = students.Count();
                var pagedStudents = students.Skip(pageIndex * pageSize).Take(pageSize).ToList();

                var studentResponses = pagedStudents.Select(MapToStudentResponse).ToList();

                return PagedApiResponse<List<StudentResponse>>.SuccessPagedResponse(
                    studentResponses,
                    "Students retrieved successfully",
                    totalCount,
                    pageIndex,
                    pageSize);
            }
            catch (Exception ex)
            {
                return PagedApiResponse<List<StudentResponse>>.ErrorPagedResponse(
                    new List<string> { ex.Message },
                    "Error retrieving students",
                    pageIndex,
                    pageSize);
            }
        }

        private StudentResponse MapToStudentResponse(Student student)
        {
            return new StudentResponse
            {
                StudentId = student.StudentID,
                AccountId = student.AccountID,
                ParentId = student.ParentID,
                ClassId = student.ClassID,
                FullName = student.FullName ?? string.Empty,
                Email = student.Account?.Email ?? string.Empty,
                FirstName = student.Account?.FirstName ?? string.Empty,
                LastName = student.Account?.LastName ?? string.Empty,
                AvatarUrl = student.Account?.AvatarUrl ?? string.Empty,
                IsActive = student.Account?.IsActive ?? false,
                CreatedAt = student.Account?.CreatedAt,
                UpdatedAt = student.Account?.UpdatedAt,
                DateOfBirth = student.DateOfBirth,
                Gender = student.Gender ?? string.Empty,
                ClassName = student.Class?.ClassName ?? string.Empty,
                ParentName = student.Parent?.FullName ?? string.Empty
            };
        }
    }
}
