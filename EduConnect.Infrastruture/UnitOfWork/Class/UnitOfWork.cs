using EduConnect.Data;
using EduConnect.Infrastruture.Repositories.Class;
using EduConnect.Infrastruture.Repositories.Interface;
using EduConnect.Infrastruture.UnitOfWork.Interface;
using System.Threading.Tasks;

namespace EduConnect.Infrastruture.UnitOfWork.Class
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly EduConnectDbContext _context;
        public IAuthRepository AuthRepository { get; }
        public UnitOfWork(EduConnectDbContext context)
        {
            _context = context;
            AuthRepository = new AuthRepository(_context, null, null, null);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
