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

        public async Task<List<EduConnect.Domain.Entities.Class>> GetClassesByTeacherIdAsync(string teacherId, int? timetableSlotId = null)
        {
            var query = _context.TimetableSlots
                .Include(ts => ts.Timetable)
                .ThenInclude(t => t.Class)
                .Where(ts => ts.TeacherID == teacherId && ts.IsDeleted != true)
                .Where(ts => ts.Timetable != null && ts.Timetable.Class != null);

            // Nếu có timetableSlotId thì filter theo nó
            if (timetableSlotId.HasValue)
            {
                query = query.Where(ts => ts.TimetableSlotID == timetableSlotId.Value);
            }

            var classes = await query
                .Select(ts => ts.Timetable.Class)
                .Distinct()
                .ToListAsync();

            return classes;
        }

        public async Task<List<(EduConnect.Domain.Entities.Class Class, int TimetableSlotId)>> GetClassesWithTimetableSlotIdByTeacherIdAsync(string teacherId, int? timetableSlotId = null)
        {
            var query = _context.TimetableSlots
                .Include(ts => ts.Timetable)
                .ThenInclude(t => t.Class)
                .Where(ts => ts.TeacherID == teacherId && ts.IsDeleted != true)
                .Where(ts => ts.Timetable != null && ts.Timetable.Class != null);

            // Nếu có timetableSlotId thì filter theo nó
            if (timetableSlotId.HasValue)
            {
                query = query.Where(ts => ts.TimetableSlotID == timetableSlotId.Value);
            }

            var classesWithSlotId = await query
                .Select(ts => new { Class = ts.Timetable.Class, TimetableSlotId = ts.TimetableSlotID })
                .ToListAsync();

            return classesWithSlotId.Select(x => (x.Class, x.TimetableSlotId)).ToList();
        }
    }
}
