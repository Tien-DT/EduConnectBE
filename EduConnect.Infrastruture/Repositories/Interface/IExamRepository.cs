using EduConnect.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduConnect.Infrastruture.Repositories.Interface
{
    public interface IExamRepository
    {
        Task<List<Exam>> GetExamByStudentIdAsync(string studentId, DateTime? startDate = null, DateTime? endDate = null, bool ascending = false);
        Task<List<Exam>> GetExamsByAccountIdAsync(string accountId, DateTime? startDate = null, DateTime? endDate = null, bool ascending = false);
        
        // Paged methods
        Task<(List<Exam> items, int totalCount)> GetPagedExamByStudentIdAsync(
            string studentId,
            int pageIndex,
            int pageSize,
            DateTime? startDate = null,
            DateTime? endDate = null,
            bool ascending = false);
            
        Task<(List<Exam> items, int totalCount)> GetPagedExamsByAccountIdAsync(
            string accountId,
            int pageIndex,
            int pageSize,
            DateTime? startDate = null,
            DateTime? endDate = null,
            bool ascending = false);
            
        Task CreateExamAsync(Exam exam);
        Task UpdateExamAsync(Exam exam);
        Task DeleteExamAsync(int id);
    }
}
