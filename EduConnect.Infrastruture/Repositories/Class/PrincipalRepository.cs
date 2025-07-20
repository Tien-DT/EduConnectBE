using EduConnect.Data;
using EduConnect.Domain.Entities;
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
        public async Task<bool> IsValidPrincipalAsync(string principalId)
        {
            return await _context.Principals
                .AnyAsync(p => p.PrincipalID == principalId && (p.IsDeleted == null || p.IsDeleted == false));
        }

        public async Task<Principal?> GetByAccountIdAsync(string accountId)
        {
            return await _context.Principals
                .FirstOrDefaultAsync(p => p.AccountID == accountId && (p.IsDeleted == null || p.IsDeleted == false));
        }
    }
}
