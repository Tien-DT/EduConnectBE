using EduConnect.Data;
using EduConnect.Infrastruture.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduConnect.Infrastruture.Repositories.Class
{
    public class BaseRepository<T, TKey> : IBaseRepository<T, TKey> where T : class
    {
        private readonly EduConnectDbContext _context;
        public BaseRepository(EduConnectDbContext eduConnectDbContext)
        {
            _context = eduConnectDbContext;
        }
        public async Task<T> CreateAsync(T entity)
        {
            _context.Set<T>().Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> Delete(TKey id)
        {
            var entity = await _context.Set<T>().FindAsync(id);
            if (entity == null)
            {
                return false;
            }

            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<bool> UpdateAsync(TKey id, T entity)
        {
            var existing = await _context.Set<T>().FindAsync(id);
            if (existing == null)
            {
                return false;
            }
            _context.Entry(existing).CurrentValues.SetValues(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<T> UpdateTAsync(TKey id, T entity)
        {
            var existing = await _context.Set<T>().FindAsync(id);
            if (existing == null)
            {
                return null;
            }
            _context.Entry(existing).CurrentValues.SetValues(entity);
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<T?> GetByIdAsync(TKey id)
        {
            return await _context.Set<T>().FindAsync(id);
        }
    }
}
