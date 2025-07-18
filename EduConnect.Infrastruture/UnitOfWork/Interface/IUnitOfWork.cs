using EduConnect.Infrastruture.Repositories.Interface;
using System.Threading.Tasks;

namespace EduConnect.Infrastruture.UnitOfWork.Interface
{
    public interface IUnitOfWork
    {
        IAuthRepository AuthRepository { get; }
        Task<int> SaveChangesAsync();
    }
}
