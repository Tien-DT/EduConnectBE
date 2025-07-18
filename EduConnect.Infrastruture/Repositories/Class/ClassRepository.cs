using EduConnect.Data;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClassEntity = EduConnect.Domain.Entities.Class;

namespace EduConnect.Infrastruture.Repositories.Class
{
    public class ClassRepository : BaseRepository<ClassEntity, int>, IClassRepository
    {
        public ClassRepository(EduConnectDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ClassEntity>> GetAllActiveClassesAsync()
        {
            return await _context.Classes
                .Where(c => c.IsDeleted != true)
                .ToListAsync();
        }

        public async Task<ClassEntity?> GetActiveClassByIdAsync(int classId)
        {
            return await _context.Classes
                .Where(c => c.ClassID == classId && c.IsDeleted != true)
                .FirstOrDefaultAsync();
        }
    }
}
