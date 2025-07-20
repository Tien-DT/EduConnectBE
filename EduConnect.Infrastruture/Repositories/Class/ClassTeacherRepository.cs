using EduConnect.Data;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduConnect.Infrastruture.Repositories.Class
{
    public class ClassTeacherRepository : IClassTeacherRepository
    {
        private readonly EduConnectDbContext _context;
        public ClassTeacherRepository(EduConnectDbContext context)
        {
            _context = context;
        }
        public async Task<bool> IsTeacherAssignedToClassAsync(int classId, string teacherId)
        {
            return await _context.ClassTeachers
                .AnyAsync(ct => ct.ClassID == classId && ct.Teacher.TeacherID == teacherId);
        }

        public async Task<List<ClassTeacher>> GetTeachersByClassIdAsync(int classId)
        {
            return await _context.ClassTeachers
                .Where(ct => ct.ClassID == classId)
                .Include(ct => ct.Teacher)
                .ToListAsync();
        }

        public async Task<List<ClassTeacher>> GetClassesByTeacherAccountIdAsync(string teacherAccountId)
        {
            return await _context.ClassTeachers
                .Where(ct => ct.Teacher.AccountID == teacherAccountId)
                .Include(ct => ct.Class)
                .ToListAsync();
        }
    }
}
