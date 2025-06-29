using EduConnect.Data;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Repositories.Class;
using EduConnect.Infrastruture.Repositories.Interface;
using EduConnect.Infrastruture.UnitOfWork.Interface;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace EduConnect.Infrastruture.UnitOfWork.Class
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly EduConnectDbContext _context;
        public IAuthRepository AuthRepository { get; }
        
        public UnitOfWork(
            EduConnectDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            AuthRepository = new AuthRepository(_context, userManager, signInManager, roleManager);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
