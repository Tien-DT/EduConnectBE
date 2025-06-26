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
    public class TimetableRepository : ITimetableRepository
    {
        private readonly EduConnectDbContext _context;
        
        public TimetableRepository(EduConnectDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ClassExistsAsync(int classId)
        {
            return await _context.Classes
                .AnyAsync(c => c.ClassID == classId && (c.IsDeleted != true));
        }

        public async Task<bool> TeacherExistsAsync(string teacherId)
        {
            return await _context.Teachers
                .AnyAsync(t => t.TeacherID == teacherId && (t.IsDeleted != true));
        }

        public async Task<bool> SubjectExistsAsync(int subjectId)
        {
            return await _context.Subjects
                .AnyAsync(s => s.SubjectId == subjectId && (s.IsDeleted != true));
        }

        public async Task<Timetable> CreateTimetableAsync(Timetable timetable)
        {
            bool classExists = await ClassExistsAsync(timetable.ClassID);
            if (!classExists)
            {
                throw new InvalidOperationException($"Class with ID {timetable.ClassID} does not exist");
            }
            foreach (var slot in timetable.TimetableSlots)
            {
                bool teacherExists = await TeacherExistsAsync(slot.TeacherID);
                if (!teacherExists)
                {
                    throw new InvalidOperationException($"Teacher with ID {slot.TeacherID} does not exist");
                }

                bool subjectExists = await SubjectExistsAsync(slot.SubjectId);
                if (!subjectExists)
                {
                    throw new InvalidOperationException($"Subject with ID {slot.SubjectId} does not exist");
                }
            }
            await _context.Timetables.AddAsync(timetable);
            await _context.SaveChangesAsync();
            return await GetTimetableByIdAsync(timetable.TimetableID);
        }

        public async Task<Timetable?> GetTimetableByIdAsync(int timetableId)
        {
            return await _context.Timetables
                .Include(t => t.TimetableSlots)
                    .ThenInclude(ts => ts.Subject)
                .Include(t => t.TimetableSlots)
                    .ThenInclude(ts => ts.Teacher)
                .Include(t => t.Class)
                .FirstOrDefaultAsync(t => t.TimetableID == timetableId && (t.IsDeleted != true));
        }

        public async Task<List<Timetable>> GetTimetablesByClassIdAsync(int classId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Timetables
                .Include(t => t.TimetableSlots.Where(ts => ts.IsDeleted != true))
                    .ThenInclude(ts => ts.Subject)
                .Include(t => t.TimetableSlots.Where(ts => ts.IsDeleted != true))
                    .ThenInclude(ts => ts.Teacher)
                .Include(t => t.Class)
                .Where(t => t.ClassID == classId && (t.IsDeleted != true));

            if (startDate.HasValue)
            {
                query = query.Where(t => t.EndDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(t => t.StartDate <= endDate.Value);
            }

            return await query.OrderByDescending(t => t.StartDate).ToListAsync();
        }

        public async Task<bool> DeleteTimetableAsync(int timetableId)
        {
            var timetable = await _context.Timetables.FindAsync(timetableId);
            if (timetable == null)
            {
                return false;
            }

            timetable.IsDeleted = true;
            
            var timetableSlots = await _context.TimetableSlots
                .Where(ts => ts.TimetableID == timetableId)
                .ToListAsync();
            
            foreach (var slot in timetableSlots)
            {
                slot.IsDeleted = true;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsTimetableExistsForClassAndDateRangeAsync(int classId, DateTime startDate, DateTime endDate)
        {
            return await _context.Timetables
                .AnyAsync(t => 
                    t.ClassID == classId && 
                    (t.IsDeleted != true) &&
                    ((t.StartDate <= endDate && t.EndDate >= startDate) || 
                     (t.StartDate >= startDate && t.EndDate <= endDate) ||
                     (t.StartDate <= startDate && t.EndDate >= endDate))
                );
        }
    }
}