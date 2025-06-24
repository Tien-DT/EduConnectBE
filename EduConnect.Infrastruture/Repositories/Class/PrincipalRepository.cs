using EduConnect.Data;
using EduConnect.Infrastruture.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EduConnect.Infrastruture.Repositories.Class
{
    public class PrincipalRepository : IPrincipalRepository
    {
        private readonly EduConnectDbContext _context;
        public PrincipalRepository(EduConnectDbContext context)
        {
            _context = context;
        }
        public async Task<bool> IsValidPrincipalAsync(string principalAccountId)
        {
            return await _context.Principals
                .AnyAsync(p => p.AccountID == principalAccountId && (p.IsDeleted == null || p.IsDeleted == false));
        }
    }
}
