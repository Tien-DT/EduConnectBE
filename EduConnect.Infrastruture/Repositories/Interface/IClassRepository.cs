using EduConnect.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClassEntity = EduConnect.Domain.Entities.Class;

namespace EduConnect.Infrastruture.Repositories.Interface
{
    public interface IClassRepository : IBaseRepository<ClassEntity, int>
    {
        Task<IEnumerable<ClassEntity>> GetAllActiveClassesAsync();
        Task<ClassEntity?> GetActiveClassByIdAsync(int classId);
    }
}
