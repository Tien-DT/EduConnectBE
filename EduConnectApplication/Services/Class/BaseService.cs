using EduConnect.Application.Services.Interface;
using EduConnect.Infrastruture.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduConnect.Application.Services.Class
{
    public class BaseService<T, TKey> : IBaseService<T, TKey> where T : class
    {
        protected readonly IBaseRepository<T, TKey> _repository;

        public BaseService(IBaseRepository<T, TKey> repository)
        {
            _repository = repository;
        }

        public virtual Task<T?> GetByIdAsync(TKey id) => _repository.GetByIdAsync(id);
        public virtual Task<IEnumerable<T>> GetAllAsync() => _repository.GetAllAsync();
        public virtual Task<T> AddAsync(T entity) => _repository.CreateAsync(entity);
        public virtual Task<T?> UpdateAsync(TKey id, T entity) => _repository.UpdateAsync(id, entity);
        public virtual Task<bool> DeleteAsync(TKey id) => _repository.DeleteAsync(id);
    }
}
