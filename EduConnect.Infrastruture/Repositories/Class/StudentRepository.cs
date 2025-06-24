using EduConnect.Data;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduConnect.Infrastruture.Repositories.Class
{
    public class StudentRepository: IBaseRepository<Student, string>, IStudentRepository
    {
        private readonly EduConnectDbContext _context;
        public StudentRepository(EduConnectDbContext context)
        {
            _context = context;
        }
        public Task<Student> CreateAsync(Student entity)
        {
            _context.Students.Add(entity);
            _context.SaveChanges();
            return Task.FromResult(entity);
        }

        public Task<bool> DeleteAsync(string id)
        {
            var student = _context.Students.Find(id);
            if (student == null)
            {
                return Task.FromResult(false);
            }
            _context.Students.Remove(student);
            _context.SaveChanges();
            return Task.FromResult(true);
        }

        public Task<IEnumerable<Student>> GetAllAsync()
        {
            var students = _context.Students.ToList();
            return Task.FromResult<IEnumerable<Student>>(students);
        }

        public Task<Student?> GetByIdAsync(string id)
        {
            var student = _context.Students.Find(id);
            return Task.FromResult(student);
        }

        public Task<Student?> UpdateAsync(string id, Student entity)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Student>> GetStudentsByClassIdAsync(int classId)
        {
            return await _context.Students
                .Where(s => s.ClassID == classId && (s.IsDeleted == null || s.IsDeleted == false))
                .ToListAsync();
        }

        public async Task<List<Student>> GetStudentsByIdsAsync(List<string> studentIds)
        {
            return await _context.Students
                .Where(s => studentIds.Contains(s.StudentID) && (s.IsDeleted == null || s.IsDeleted == false))
                .ToListAsync();
        }
    }
}
