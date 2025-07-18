using EduConnect.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduConnect.Infrastruture.Repositories.Interface
{
    public interface IClassTeacherRepository
    {
        Task<bool> IsTeacherAssignedToClassAsync(int classId, string teacherAccountId);
        Task<List<ClassTeacher>> GetTeachersByClassIdAsync(int classId);
        Task<List<ClassTeacher>> GetClassesByTeacherAccountIdAsync(string teacherAccountId);
    }
}