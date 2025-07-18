using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduConnect.Infrastruture.Repositories.Interface
{
    public interface IBaseRepository<T, TKey> where T : class
    {
        Task<T> CreateAsync(T entity);
        Task<T?> UpdateAsync(TKey id, T entity);  
        Task<bool> DeleteAsync(TKey id);
        Task<T?> GetByIdAsync(TKey id);
        Task<IEnumerable<T>> GetAllAsync();
    }

}
