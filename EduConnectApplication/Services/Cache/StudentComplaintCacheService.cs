using EduConnect.Application.DTOs;
using EduConnect.Application.Services.Interface;
using EduConnect.Infrastruture.Options;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

namespace EduConnect.Application.Services.Cache
{
    public class StudentComplaintCacheService : IStudentComplaintCacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly RedisOption _redisOptions;
        private const string COMPLAINT_KEY_PREFIX = "student_complaints:";

        public StudentComplaintCacheService(IDistributedCache distributedCache, IOptions<RedisOption> redisOptions)
        {
            _distributedCache = distributedCache;
            _redisOptions = redisOptions.Value;
        }

        public async Task AddStudentComplaintAsync(StudentComplaintData complaint)
        {
            try
            {
                var key = GetDailyCacheKey(complaint.Date);
                var existingDataJson = await _distributedCache.GetStringAsync(key);
                
                var dailySummary = existingDataJson != null 
                    ? JsonConvert.DeserializeObject<DailyComplaintSummary>(existingDataJson) ?? new DailyComplaintSummary { Date = complaint.Date }
                    : new DailyComplaintSummary { Date = complaint.Date };
                var existingStudent = dailySummary.Students.FirstOrDefault(s => s.StudentId == complaint.StudentId);
                if (existingStudent != null)
                {
                    existingStudent.Complaints.AddRange(complaint.Complaints);
                }
                else
                {
                    dailySummary.Students.Add(complaint);
                }

                var serializedData = JsonConvert.SerializeObject(dailySummary);
                var dataBytes = Encoding.UTF8.GetBytes(serializedData);

                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(25) 
                };

                await _distributedCache.SetAsync(key, dataBytes, options);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding student complaint to cache: {ex.Message}", ex);
            }
        }

        public async Task<List<StudentComplaintData>> GetDailyComplaintsAsync(DateTime date)
        {
            try
            {
                var key = GetDailyCacheKey(date);
                var dataJson = await _distributedCache.GetStringAsync(key);
                
                if (string.IsNullOrEmpty(dataJson))
                {
                    return new List<StudentComplaintData>();
                }

                var dailySummary = JsonConvert.DeserializeObject<DailyComplaintSummary>(dataJson);
                return dailySummary?.Students ?? new List<StudentComplaintData>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving daily complaints from cache: {ex.Message}", ex);
            }
        }

        public async Task RemoveDailyComplaintsAsync(DateTime date)
        {
            try
            {
                var key = GetDailyCacheKey(date);
                await _distributedCache.RemoveAsync(key);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error removing daily complaints from cache: {ex.Message}", ex);
            }
        }

        public async Task<bool> HasComplaintsForDateAsync(DateTime date)
        {
            try
            {
                var key = GetDailyCacheKey(date);
                var dataJson = await _distributedCache.GetStringAsync(key);
                return !string.IsNullOrEmpty(dataJson);
            }
            catch
            {
                return false;
            }
        }

        private string GetDailyCacheKey(DateTime date)
        {
            return $"{COMPLAINT_KEY_PREFIX}{date:yyyy-MM-dd}";
        }
    }
}
