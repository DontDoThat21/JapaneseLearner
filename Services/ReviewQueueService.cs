using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JapaneseTracker.Models;
using Microsoft.Extensions.Logging;

namespace JapaneseTracker.Services
{
    public interface IReviewQueueService
    {
        Task<List<ReviewQueue>> GetDueReviewsAsync(int userId, int maxItems = 20);
        Task<List<ReviewQueue>> GetUpcomingReviewsAsync(int userId, int days = 7);
        Task<ReviewQueue> AddToQueueAsync(int userId, string itemType, int itemId, DateTime? scheduledDate = null);
        Task<bool> CompleteReviewAsync(int queueId, bool isCorrect, int difficultyRating = 1);
        Task<ReviewSession> StartReviewSessionAsync(int userId);
        Task<bool> EndReviewSessionAsync(int sessionId);
        Task<List<ReviewQueue>> GetReviewsByPriorityAsync(int userId, int priority);
        Task<int> GetTotalDueCountAsync(int userId);
        Task<Dictionary<string, int>> GetReviewStatsAsync(int userId);
    }
    
    public class ReviewQueueService : IReviewQueueService
    {
        private readonly DatabaseService _databaseService;
        private readonly SRSCalculationService _srsService;
        private readonly ILogger<ReviewQueueService> _logger;
        
        public ReviewQueueService(
            DatabaseService databaseService,
            SRSCalculationService srsService,
            ILogger<ReviewQueueService> logger)
        {
            _databaseService = databaseService;
            _srsService = srsService;
            _logger = logger;
        }
        
        public async Task<List<ReviewQueue>> GetDueReviewsAsync(int userId, int maxItems = 20)
        {
            try
            {
                var context = await _databaseService.GetDbContextAsync();
                
                var dueReviews = context.ReviewQueue
                    .Where(r => r.UserId == userId && 
                               r.ScheduledDate <= DateTime.UtcNow && 
                               !r.IsCompleted)
                    .OrderBy(r => r.Priority)
                    .ThenBy(r => r.ScheduledDate)
                    .Take(maxItems)
                    .ToList();
                
                _logger.LogDebug($"Found {dueReviews.Count} due reviews for user {userId}");
                return dueReviews;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to get due reviews for user {userId}");
                return new List<ReviewQueue>();
            }
        }
        
        public async Task<List<ReviewQueue>> GetUpcomingReviewsAsync(int userId, int days = 7)
        {
            try
            {
                var context = await _databaseService.GetDbContextAsync();
                var endDate = DateTime.UtcNow.AddDays(days);
                
                var upcomingReviews = context.ReviewQueue
                    .Where(r => r.UserId == userId && 
                               r.ScheduledDate > DateTime.UtcNow && 
                               r.ScheduledDate <= endDate && 
                               !r.IsCompleted)
                    .OrderBy(r => r.ScheduledDate)
                    .ToList();
                
                _logger.LogDebug($"Found {upcomingReviews.Count} upcoming reviews for user {userId}");
                return upcomingReviews;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to get upcoming reviews for user {userId}");
                return new List<ReviewQueue>();
            }
        }
        
        public async Task<ReviewQueue> AddToQueueAsync(int userId, string itemType, int itemId, DateTime? scheduledDate = null)
        {
            try
            {
                var context = await _databaseService.GetDbContextAsync();
                
                // Check if item already exists in queue
                var existingItem = context.ReviewQueue
                    .FirstOrDefault(r => r.UserId == userId && 
                                        r.ItemType == itemType && 
                                        r.ItemId == itemId && 
                                        !r.IsCompleted);
                
                if (existingItem != null)
                {
                    // Update existing item
                    if (scheduledDate.HasValue)
                    {
                        existingItem.ScheduledDate = scheduledDate.Value;
                    }
                    await context.SaveChangesAsync();
                    return existingItem;
                }
                
                // Create new queue item
                var queueItem = new ReviewQueue
                {
                    UserId = userId,
                    ItemType = itemType,
                    ItemId = itemId,
                    ScheduledDate = scheduledDate ?? DateTime.UtcNow.AddDays(1),
                    Priority = 0, // Default to low priority
                    SRSLevel = 0
                };
                
                context.ReviewQueue.Add(queueItem);
                await context.SaveChangesAsync();
                
                _logger.LogDebug($"Added {itemType} item {itemId} to review queue for user {userId}");
                return queueItem;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to add item to queue: {itemType} {itemId} for user {userId}");
                throw;
            }
        }
        
        public async Task<bool> CompleteReviewAsync(int queueId, bool isCorrect, int difficultyRating = 1)
        {
            try
            {
                var context = await _databaseService.GetDbContextAsync();
                
                var queueItem = context.ReviewQueue.Find(queueId);
                if (queueItem == null)
                {
                    _logger.LogWarning($"Queue item {queueId} not found");
                    return false;
                }
                
                // Mark as completed
                queueItem.CompletedDate = DateTime.UtcNow;
                
                // Calculate next review date using SRS
                var difficulty = (ReviewDifficulty)difficultyRating;
                var nextReviewDate = _srsService.CalculateNextReview(queueItem.SRSLevel, isCorrect, difficulty);
                var newSRSLevel = _srsService.CalculateNewSRSLevel(queueItem.SRSLevel, isCorrect, difficulty);
                
                // Create new queue item for next review (unless mastered)
                if (!_srsService.IsItemMastered(newSRSLevel, isCorrect ? 100 : 0))
                {
                    var nextReviewItem = new ReviewQueue
                    {
                        UserId = queueItem.UserId,
                        ItemType = queueItem.ItemType,
                        ItemId = queueItem.ItemId,
                        ScheduledDate = nextReviewDate,
                        Priority = CalculatePriority(newSRSLevel, isCorrect),
                        SRSLevel = newSRSLevel
                    };
                    
                    context.ReviewQueue.Add(nextReviewItem);
                }
                
                await context.SaveChangesAsync();
                
                _logger.LogDebug($"Completed review for queue item {queueId}, next review: {nextReviewDate}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to complete review for queue item {queueId}");
                return false;
            }
        }
        
        public async Task<ReviewSession> StartReviewSessionAsync(int userId)
        {
            try
            {
                var context = await _databaseService.GetDbContextAsync();
                
                var session = new ReviewSession
                {
                    UserId = userId,
                    StartTime = DateTime.UtcNow
                };
                
                context.ReviewSessions.Add(session);
                await context.SaveChangesAsync();
                
                _logger.LogDebug($"Started review session {session.SessionId} for user {userId}");
                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to start review session for user {userId}");
                throw;
            }
        }
        
        public async Task<bool> EndReviewSessionAsync(int sessionId)
        {
            try
            {
                var context = await _databaseService.GetDbContextAsync();
                
                var session = context.ReviewSessions.Find(sessionId);
                if (session == null)
                {
                    _logger.LogWarning($"Review session {sessionId} not found");
                    return false;
                }
                
                session.EndTime = DateTime.UtcNow;
                await context.SaveChangesAsync();
                
                _logger.LogDebug($"Ended review session {sessionId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to end review session {sessionId}");
                return false;
            }
        }
        
        public async Task<List<ReviewQueue>> GetReviewsByPriorityAsync(int userId, int priority)
        {
            try
            {
                var context = await _databaseService.GetDbContextAsync();
                
                var reviews = context.ReviewQueue
                    .Where(r => r.UserId == userId && r.Priority == priority && !r.IsCompleted)
                    .OrderBy(r => r.ScheduledDate)
                    .ToList();
                
                return reviews;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to get reviews by priority {priority} for user {userId}");
                return new List<ReviewQueue>();
            }
        }
        
        public async Task<int> GetTotalDueCountAsync(int userId)
        {
            try
            {
                var context = await _databaseService.GetDbContextAsync();
                
                var count = context.ReviewQueue
                    .Count(r => r.UserId == userId && 
                               r.ScheduledDate <= DateTime.UtcNow && 
                               !r.IsCompleted);
                
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to get total due count for user {userId}");
                return 0;
            }
        }
        
        public async Task<Dictionary<string, int>> GetReviewStatsAsync(int userId)
        {
            try
            {
                var context = await _databaseService.GetDbContextAsync();
                
                var stats = new Dictionary<string, int>();
                
                // Due today
                stats["DueToday"] = context.ReviewQueue
                    .Count(r => r.UserId == userId && 
                               r.ScheduledDate.Date <= DateTime.UtcNow.Date && 
                               !r.IsCompleted);
                
                // Overdue
                stats["Overdue"] = context.ReviewQueue
                    .Count(r => r.UserId == userId && 
                               r.ScheduledDate < DateTime.UtcNow.Date && 
                               !r.IsCompleted);
                
                // Upcoming (next 7 days)
                var nextWeek = DateTime.UtcNow.AddDays(7);
                stats["Upcoming"] = context.ReviewQueue
                    .Count(r => r.UserId == userId && 
                               r.ScheduledDate > DateTime.UtcNow && 
                               r.ScheduledDate <= nextWeek && 
                               !r.IsCompleted);
                
                // By type
                var itemTypes = new[] { "Kanji", "Vocabulary", "Grammar", "Kana" };
                foreach (var itemType in itemTypes)
                {
                    stats[$"Due{itemType}"] = context.ReviewQueue
                        .Count(r => r.UserId == userId && 
                                   r.ItemType == itemType && 
                                   r.ScheduledDate <= DateTime.UtcNow && 
                                   !r.IsCompleted);
                }
                
                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to get review stats for user {userId}");
                return new Dictionary<string, int>();
            }
        }
        
        private int CalculatePriority(int srsLevel, bool wasCorrect)
        {
            // Higher priority for items that were answered incorrectly
            if (!wasCorrect)
                return 3; // Critical
            
            // Lower SRS levels get higher priority
            if (srsLevel < 2)
                return 2; // High
            if (srsLevel < 4)
                return 1; // Medium
            
            return 0; // Low
        }
    }
}