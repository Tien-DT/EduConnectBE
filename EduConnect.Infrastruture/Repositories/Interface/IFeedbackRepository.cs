using EduConnect.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduConnect.Infrastruture.Repositories.Interface
{
    public interface IFeedbackRepository : IBaseRepository<Feedback, int>
    {
        Task<(List<Feedback> items, int totalCount)> GetFeedbacksAsync(
            int pageIndex = 0,
            int pageSize = 10,
            string? searchTerm = null,
            string? sortBy = "DateTime",
            bool ascending = false,
            string? targetFilter = null);

        Task<(List<Feedback> items, int totalCount)> GetFeedbacksByParentIdAsync(string parentId, int pageIndex = 0, int pageSize = 10);
        Task<List<Feedback>> GetFeedbacksByTargetAsync(string target);
    }
}
