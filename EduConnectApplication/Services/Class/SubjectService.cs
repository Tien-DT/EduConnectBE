using AutoMapper;
using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using EduConnect.Application.Services.Interface;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduConnect.Application.Services.Class
{
    public class SubjectService : ISubjectService
    {
        private readonly ISubjectRepository _subjectRepository;
        private readonly IMapper _mapper;

        public SubjectService(
            ISubjectRepository subjectRepository,
            IMapper mapper)
        {
            _subjectRepository = subjectRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<SubjectResponse>> GetSubjectByIdAsync(int subjectId)
        {
            try
            {
                var subject = await _subjectRepository.GetByIdAsync(subjectId);
                
                if (subject == null)
                {
                    return ApiResponse<SubjectResponse>.ErrorResponse(
                        new List<string> { "Subject not found." },
                        "Subject not found");
                }

                var response = _mapper.Map<SubjectResponse>(subject);
                return ApiResponse<SubjectResponse>.SuccessResponse(response, "Subject retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<SubjectResponse>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Error retrieving subject");
            }
        }

        public async Task<ApiResponse<List<SubjectResponse>>> GetAllSubjectsAsync()
        {
            try
            {
                var subjects = await _subjectRepository.GetAllAsync();
                var responses = _mapper.Map<List<SubjectResponse>>(subjects);
                
                return ApiResponse<List<SubjectResponse>>.SuccessResponse(responses, "Subjects retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<SubjectResponse>>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Error retrieving subjects");
            }
        }

        public async Task<ApiResponse<SubjectResponse>> CreateSubjectAsync(CreateSubjectRequest request)
        {
            try
            {
                // Check if subject with same name exists
                if (await _subjectRepository.ExistsByNameAsync(request.SubjectName))
                {
                    return ApiResponse<SubjectResponse>.ErrorResponse(
                        new List<string> { "Subject with this name already exists." },
                        "Duplicate subject name");
                }

                // Check if subject with same code exists (if code is provided)
                if (!string.IsNullOrWhiteSpace(request.SubjectCode) && 
                    await _subjectRepository.ExistsByCodeAsync(request.SubjectCode))
                {
                    return ApiResponse<SubjectResponse>.ErrorResponse(
                        new List<string> { "Subject with this code already exists." },
                        "Duplicate subject code");
                }

                var subject = _mapper.Map<Subject>(request);
                subject.IsDeleted = false;
                
                var createdSubject = await _subjectRepository.AddAsync(subject);
                var response = _mapper.Map<SubjectResponse>(createdSubject);
                
                return ApiResponse<SubjectResponse>.SuccessResponse(response, "Subject created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<SubjectResponse>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Error creating subject");
            }
        }

        public async Task<ApiResponse<SubjectResponse>> UpdateSubjectAsync(int subjectId, UpdateSubjectRequest request)
        {
            try
            {
                var existingSubject = await _subjectRepository.GetByIdAsync(subjectId);
                if (existingSubject == null)
                {
                    return ApiResponse<SubjectResponse>.ErrorResponse(
                        new List<string> { "Subject not found." },
                        "Subject not found");
                }

                // Check if subject with same name exists (excluding current subject)
                var subjects = await _subjectRepository.GetAllAsync();
                if (subjects.Any(s => s.SubjectName == request.SubjectName && s.SubjectId != subjectId))
                {
                    return ApiResponse<SubjectResponse>.ErrorResponse(
                        new List<string> { "Subject with this name already exists." },
                        "Duplicate subject name");
                }

                // Check if subject with same code exists (excluding current subject)
                if (!string.IsNullOrWhiteSpace(request.SubjectCode) && 
                    subjects.Any(s => s.SubjectCode == request.SubjectCode && s.SubjectId != subjectId))
                {
                    return ApiResponse<SubjectResponse>.ErrorResponse(
                        new List<string> { "Subject with this code already exists." },
                        "Duplicate subject code");
                }

                existingSubject.SubjectName = request.SubjectName;
                existingSubject.SubjectCode = request.SubjectCode;
                existingSubject.Description = request.Description;

                var updatedSubject = await _subjectRepository.UpdateAsync(existingSubject);
                var response = _mapper.Map<SubjectResponse>(updatedSubject);
                
                return ApiResponse<SubjectResponse>.SuccessResponse(response, "Subject updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<SubjectResponse>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Error updating subject");
            }
        }

        public async Task<ApiResponse<string>> DeleteSubjectAsync(int subjectId)
        {
            try
            {
                var subject = await _subjectRepository.GetByIdAsync(subjectId);
                if (subject == null)
                {
                    return ApiResponse<string>.ErrorResponse(
                        new List<string> { "Subject not found." },
                        "Subject not found");
                }

                // Check if subject has lessons or exams
                if (subject.Lessons.Any() || subject.Exams.Any())
                {
                    return ApiResponse<string>.ErrorResponse(
                        new List<string> { "Cannot delete subject with existing lessons or exams." },
                        "Subject has dependencies");
                }

                var deleted = await _subjectRepository.DeleteAsync(subjectId);
                if (!deleted)
                {
                    return ApiResponse<string>.ErrorResponse(
                        new List<string> { "Failed to delete subject." },
                        "Delete failed");
                }
                
                return ApiResponse<string>.SuccessResponse("Subject deleted successfully", "Subject deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Error deleting subject");
            }
        }

        public async Task<PagedApiResponse<List<SubjectResponse>>> FilterSortSearchSubjectsAsync(
            string? searchTerm = null,
            string? sortBy = null,
            bool ascending = true,
            int pageIndex = 1,
            int pageSize = 10,
            bool includeDeleted = false)
        {
            try
            {
                var (subjects, totalCount) = await _subjectRepository.FilterSortSearchAsync(
                    searchTerm, sortBy, ascending, pageIndex, pageSize, includeDeleted);
                
                var responses = _mapper.Map<List<SubjectResponse>>(subjects);
                
                return PagedApiResponse<List<SubjectResponse>>.SuccessPagedResponse(
                    responses,
                    "Subjects retrieved successfully",
                    totalCount,
                    pageIndex,
                    pageSize);
            }
            catch (Exception ex)
            {
                return PagedApiResponse<List<SubjectResponse>>.ErrorPagedResponse(
                    new List<string> { ex.Message },
                    "Error retrieving subjects");
            }
        }
    }
}
