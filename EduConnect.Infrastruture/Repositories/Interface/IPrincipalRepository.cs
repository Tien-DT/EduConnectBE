using EduConnect.Domain.Entities;
using System.Threading.Tasks;

namespace EduConnect.Infrastruture.Repositories.Interface
{
    public interface IPrincipalRepository
    {
        Task<bool> IsValidPrincipalAsync(string principalAccountId);
        Task<Principal?> GetByAccountIdAsync(string accountId);
    }
}