using AutoMapper;
using EduConnect.Application.DTOs.Response;
using EduConnect.Application.Services.Interface;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduConnect.Application.Services.Class
{
    public class ExamService : IExamService
    {
        private readonly IExamRepository _examRepository;
        private readonly IMapper _mapper;

        public ExamService(IExamRepository examRepository, IMapper mapper)
        {
            _examRepository = examRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<List<ExamResponse>>> GetExamsByAccountIdAsync(
            string accountId, 
            DateTime? startDate = null, 
            DateTime? endDate = null, 
            bool ascending = false,
            int? pageIndex = null,
            int? pageSize = null)
        {
            try
            {
                if (pageIndex.HasValue && pageSize.HasValue)
                {
                    var (exams, totalCount) = await _examRepository.GetPagedExamsByAccountIdAsync(
                        accountId,
                        pageIndex.Value,
                        pageSize.Value,
                        startDate,
                        endDate,
                        ascending);

                    if (exams == null || !exams.Any())
                    {
                        var emptyResponse = ApiResponse<List<ExamResponse>>.SuccessResponse(
                            new List<ExamResponse>(),
                            "Không có lịch thi trong thời gian này"
                        );
                        
                        emptyResponse.TotalCount = 0;
                        emptyResponse.PageIndex = pageIndex.Value;
                        emptyResponse.PageSize = pageSize.Value;
                        emptyResponse.TotalPages = 0;
                        
                        return emptyResponse;
                    }

                    var examResponses = _mapper.Map<List<ExamResponse>>(exams);
                    var response = ApiResponse<List<ExamResponse>>.SuccessResponse(
                        examResponses,
                        "Exams retrieved successfully"
                    );
                    
                    response.TotalCount = totalCount;
                    response.PageIndex = pageIndex.Value;
                    response.PageSize = pageSize.Value;
                    response.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize.Value);
                    
                    return response;
                }

                var allExams = await _examRepository.GetExamsByAccountIdAsync(accountId, startDate, endDate, ascending);
                if (allExams == null || !allExams.Any())
                {
                    var emptyResponse = ApiResponse<List<ExamResponse>>.SuccessResponse(
                        new List<ExamResponse>(), 
                        "Không có lịch thi trong thời gian này"
                    );
                    
                    emptyResponse.TotalCount = 0;
                    emptyResponse.PageIndex = 1;
                    emptyResponse.PageSize = allExams?.Count ?? 0;
                    emptyResponse.TotalPages = 0;
                    
                    return emptyResponse;
                }

                var allExamResponses = _mapper.Map<List<ExamResponse>>(allExams);
                var allResponse = ApiResponse<List<ExamResponse>>.SuccessResponse(
                    allExamResponses, 
                    "Exams retrieved successfully"
                );
                
                allResponse.TotalCount = allExamResponses.Count;
                allResponse.PageIndex = 1;
                allResponse.PageSize = allExamResponses.Count;
                allResponse.TotalPages = 1;
                
                return allResponse;
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<List<ExamResponse>>.ErrorResponse(
                    new List<string> { ex.Message }, 
                    "Error retrieving exams"
                );
                errorResponse.TotalCount = 0;
                errorResponse.PageIndex = pageIndex ?? 0;
                errorResponse.PageSize = pageSize ?? 0;
                errorResponse.TotalPages = 0;
                
                return errorResponse;
            }
        }

        public async Task<ApiResponse<List<ExamResponse>>> GetExamsByStudentIdAsync(
            string studentId, 
            DateTime? startDate = null, 
            DateTime? endDate = null, 
            bool ascending = false,
            int? pageIndex = null,
            int? pageSize = null)
        {
            try
            {
                if (pageIndex.HasValue && pageSize.HasValue)
                {
                    var (exams, totalCount) = await _examRepository.GetPagedExamByStudentIdAsync(
                        studentId,
                        pageIndex.Value,
                        pageSize.Value,
                        startDate,
                        endDate,
                        ascending);

                    if (exams == null || !exams.Any())
                    {
                        var emptyResponse = ApiResponse<List<ExamResponse>>.SuccessResponse(
                            new List<ExamResponse>(),
                            "Không có lịch thi trong thời gian này"
                        );
                        
                        emptyResponse.TotalCount = 0;
                        emptyResponse.PageIndex = pageIndex.Value;
                        emptyResponse.PageSize = pageSize.Value;
                        emptyResponse.TotalPages = 0;
                        
                        return emptyResponse;
                    }

                    var examResponses = _mapper.Map<List<ExamResponse>>(exams);
                    var response = ApiResponse<List<ExamResponse>>.SuccessResponse(
                        examResponses,
                        "Exams retrieved successfully"
                    );
                    
                    response.TotalCount = totalCount;
                    response.PageIndex = pageIndex.Value;
                    response.PageSize = pageSize.Value;
                    response.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize.Value);
                    
                    return response;
                }
                var allExams = await _examRepository.GetExamByStudentIdAsync(studentId, startDate, endDate, ascending);
                if (allExams == null || !allExams.Any())
                {
                    var emptyResponse = ApiResponse<List<ExamResponse>>.SuccessResponse(
                        new List<ExamResponse>(),
                        "Không có lịch thi trong thời gian này"
                    );
                    
                    emptyResponse.TotalCount = 0;
                    emptyResponse.PageIndex = 1;
                    emptyResponse.PageSize = allExams?.Count ?? 0;
                    emptyResponse.TotalPages = 0;
                    
                    return emptyResponse;
                }

                var allExamResponses = _mapper.Map<List<ExamResponse>>(allExams);
                var allResponse = ApiResponse<List<ExamResponse>>.SuccessResponse(
                    allExamResponses,
                    "Exams retrieved successfully"
                );
                
                allResponse.TotalCount = allExamResponses.Count;
                allResponse.PageIndex = 1;
                allResponse.PageSize = allExamResponses.Count;
                allResponse.TotalPages = 1;
                
                return allResponse;
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<List<ExamResponse>>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Error retrieving exams"
                );
                errorResponse.TotalCount = 0;
                errorResponse.PageIndex = pageIndex ?? 0;
                errorResponse.PageSize = pageSize ?? 0;
                errorResponse.TotalPages = 0;
                
                return errorResponse;
            }
        }
    }
}