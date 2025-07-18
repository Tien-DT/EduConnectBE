using EduConnect.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduConnect.Infrastruture.Repositories.Interface
{
    public interface ITimetableSlotRepository
    {
        Task<List<TimetableSlot>> GetTimetableSlotsByTeacherIdAsync(string teacherId, DateTime? startDate = null, DateTime? endDate = null);
    }
}
