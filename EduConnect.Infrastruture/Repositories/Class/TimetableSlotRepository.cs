using EduConnect.Data;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduConnect.Infrastruture.Repositories.Class
{
    public class TimetableSlotRepository : ITimetableSlotRepository
    {
        private readonly EduConnectDbContext _context;
        public TimetableSlotRepository(EduConnectDbContext context)
        {
            _context = context;
        }
        public async Task<List<TimetableSlot>> GetTimetableSlotsByTeacherIdAsync(string teacherId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.TimetableSlots
                .Include(ts => ts.Subject)
                .Include(ts => ts.Teacher)
                .Include(ts => ts.Timetable)
                .Where(ts => ts.TeacherID == teacherId && (ts.IsDeleted != true));

            if (startDate.HasValue || endDate.HasValue)
            {
                query = query.Where(ts => 
                    (!startDate.HasValue || ts.Timetable.EndDate >= startDate.Value) && 
                    (!endDate.HasValue || ts.Timetable.StartDate <= endDate.Value));
            }

            return await query.ToListAsync();
        }

        public async Task<List<EduConnect.Domain.Entities.Class>> GetClassesByTeacherIdAsync(string teacherId)
        {
            var classes = await _context.TimetableSlots
                .Include(ts => ts.Timetable)
                .ThenInclude(t => t.Class)
                .Where(ts => ts.TeacherID == teacherId && ts.IsDeleted != true)
                .Where(ts => ts.Timetable != null && ts.Timetable.Class != null)
                .Select(ts => ts.Timetable.Class)
                .Distinct()
                .ToListAsync();

            return classes;
        }
    }
}
