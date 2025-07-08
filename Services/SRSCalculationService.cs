using Microsoft.Extensions.Configuration;

namespace JapaneseTracker.Services
{
    public class SRSCalculationService
    {
        private readonly int[] _intervals;
        private readonly int _initialInterval;
        private readonly double _easyBonus;
        private readonly double _hardPenalty;
        
        public SRSCalculationService(IConfiguration configuration)
        {
            _intervals = configuration.GetSection("SRS:Intervals").Get<int[]>() ?? new[] { 1, 3, 7, 14, 30, 90, 180, 365 };
            _initialInterval = int.Parse(configuration["SRS:InitialInterval"] ?? "1");
            _easyBonus = double.Parse(configuration["SRS:EasyBonus"] ?? "1.3");
            _hardPenalty = double.Parse(configuration["SRS:HardPenalty"] ?? "0.6");
        }
        
        public DateTime CalculateNextReview(int currentLevel, bool wasCorrect, ReviewDifficulty difficulty = ReviewDifficulty.Normal)
        {
            int newLevel = CalculateNewSRSLevel(currentLevel, wasCorrect, difficulty);
            int intervalDays = GetIntervalForLevel(newLevel);
            
            return DateTime.UtcNow.AddDays(intervalDays);
        }
        
        public int CalculateNewSRSLevel(int currentLevel, bool wasCorrect, ReviewDifficulty difficulty = ReviewDifficulty.Normal)
        {
            if (!wasCorrect)
            {
                // If incorrect, drop the level significantly
                return Math.Max(0, currentLevel - 2);
            }
            
            // If correct, advance based on difficulty
            int advancement = difficulty switch
            {
                ReviewDifficulty.Easy => 2,
                ReviewDifficulty.Hard => 0, // Stay at current level
                _ => 1 // Normal advancement
            };
            
            return Math.Min(currentLevel + advancement, _intervals.Length - 1);
        }
        
        public int GetIntervalForLevel(int level)
        {
            if (level < 0 || level >= _intervals.Length)
            {
                return _initialInterval;
            }
            
            return _intervals[level];
        }
        
        public double CalculateRetentionRate(int correctCount, int totalCount)
        {
            return totalCount > 0 ? (double)correctCount / totalCount * 100 : 0;
        }
        
        public SRSStats CalculateSRSStats(int correctCount, int incorrectCount, int currentLevel)
        {
            var totalReviews = correctCount + incorrectCount;
            var retentionRate = CalculateRetentionRate(correctCount, totalReviews);
            var nextReviewDays = GetIntervalForLevel(currentLevel);
            
            return new SRSStats
            {
                TotalReviews = totalReviews,
                CorrectReviews = correctCount,
                IncorrectReviews = incorrectCount,
                RetentionRate = retentionRate,
                CurrentLevel = currentLevel,
                NextReviewDays = nextReviewDays,
                MasteryLevel = GetMasteryLevel(currentLevel, retentionRate)
            };
        }
        
        public string GetMasteryLevel(int srsLevel, double retentionRate)
        {
            if (srsLevel >= 7 && retentionRate >= 90)
                return "Master";
            if (srsLevel >= 5 && retentionRate >= 80)
                return "Advanced";
            if (srsLevel >= 3 && retentionRate >= 70)
                return "Intermediate";
            if (srsLevel >= 1 && retentionRate >= 60)
                return "Beginner";
            
            return "Learning";
        }
        
        public bool IsItemMastered(int srsLevel, double retentionRate)
        {
            return srsLevel >= 6 && retentionRate >= 85;
        }
        
        public ReviewPriority GetReviewPriority(DateTime nextReviewDate, int srsLevel, double retentionRate)
        {
            var daysOverdue = (DateTime.UtcNow - nextReviewDate).Days;
            
            if (daysOverdue > 7)
                return ReviewPriority.Critical;
            if (daysOverdue > 3)
                return ReviewPriority.High;
            if (daysOverdue > 0)
                return ReviewPriority.Medium;
            
            // If not overdue, consider retention rate
            if (retentionRate < 60)
                return ReviewPriority.High;
            if (retentionRate < 80)
                return ReviewPriority.Medium;
            
            return ReviewPriority.Low;
        }
        
        public List<DateTime> GetUpcomingReviewDates(int srsLevel, int count = 5)
        {
            var dates = new List<DateTime>();
            var currentDate = DateTime.UtcNow;
            
            for (int i = 0; i < count; i++)
            {
                var intervalDays = GetIntervalForLevel(srsLevel + i);
                currentDate = currentDate.AddDays(intervalDays);
                dates.Add(currentDate);
            }
            
            return dates;
        }
    }
    
    public class SRSStats
    {
        public int TotalReviews { get; set; }
        public int CorrectReviews { get; set; }
        public int IncorrectReviews { get; set; }
        public double RetentionRate { get; set; }
        public int CurrentLevel { get; set; }
        public int NextReviewDays { get; set; }
        public string MasteryLevel { get; set; } = string.Empty;
    }
    
    public enum ReviewDifficulty
    {
        Easy,
        Normal,
        Hard
    }
    
    public enum ReviewPriority
    {
        Low,
        Medium,
        High,
        Critical
    }
}
