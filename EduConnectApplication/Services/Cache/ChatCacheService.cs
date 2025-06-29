using EduConnect.Application.DTOs.Response;
using EduConnect.Application.Services.Interface;
using EduConnect.Infrastruture.Options;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduConnect.Application.Services.Cache
{
    public class ChatCacheService : IChatCacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly RedisOption _redisOptions;
        private const string CHAT_HISTORY_PREFIX = "chat_history:";

        public ChatCacheService(IDistributedCache distributedCache, IOptions<RedisOption> redisOptions)
        {
            _distributedCache = distributedCache;
            _redisOptions = redisOptions.Value;
        }

        public async Task<List<ChatHistoryEntry>> GetChatHistoryAsync(string userId)
        {
            try
            {
                var key = GetCacheKey(userId);
                var cachedData = await _distributedCache.GetStringAsync(key);

                if (string.IsNullOrEmpty(cachedData))
                {
                    return new List<ChatHistoryEntry>();
                }

                var chatHistory = JsonConvert.DeserializeObject<List<ChatHistoryEntry>>(cachedData);
                return chatHistory ?? new List<ChatHistoryEntry>();
            }
            catch (Exception)
            {
                return new List<ChatHistoryEntry>();
            }
        }

        public async Task AddMessageToChatHistoryAsync(string userId, string role, string message)
        {
            try
            {
                var chatHistory = await GetChatHistoryAsync(userId);
                
                chatHistory.Add(new ChatHistoryEntry
                {
                    Role = role,
                    Message = message,
                    Timestamp = DateTime.UtcNow
                });

                if (chatHistory.Count > 50)
                {
                    chatHistory = chatHistory.TakeLast(50).ToList();
                }

                await SaveChatHistoryAsync(userId, chatHistory);
            }
            catch (Exception)
            {
               
            }
        }

        public async Task ClearChatHistoryAsync(string userId)
        {
            try
            {
                var key = GetCacheKey(userId);
                await _distributedCache.RemoveAsync(key);
            }
            catch (Exception)
            {
                
            }
        }

        public async Task ExtendChatHistoryExpirationAsync(string userId)
        {
            try
            {
                var chatHistory = await GetChatHistoryAsync(userId);
                if (chatHistory.Any())
                {
                    await SaveChatHistoryAsync(userId, chatHistory);
                }
            }
            catch (Exception)
            {
               
            }
        }

        private async Task SaveChatHistoryAsync(string userId, List<ChatHistoryEntry> chatHistory)
        {
            var key = GetCacheKey(userId);
            var serializedData = JsonConvert.SerializeObject(chatHistory);
            
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_redisOptions.DefaultExpirationTimeInMinutes)
            };

            await _distributedCache.SetStringAsync(key, serializedData, options);
        }

        private string GetCacheKey(string userId)
        {
            return $"{CHAT_HISTORY_PREFIX}{userId}";
        }
    }
}
