using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JapaneseTracker.Models
{
    public class ReviewQueue
    {
        [Key]
        public int QueueId { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string ItemType { get; set; } = string.Empty; // Kanji, Vocabulary, Grammar, Kana
        
        [Required]
        public int ItemId { get; set; }
        
        public DateTime ScheduledDate { get; set; }
        
        public DateTime? CompletedDate { get; set; }
        
        public int Priority { get; set; } = 0; // 0 = Low, 1 = Medium, 2 = High, 3 = Critical
        
        public int SRSLevel { get; set; } = 0;
        
        public bool IsCompleted => CompletedDate.HasValue;
        
        public bool IsOverdue => DateTime.UtcNow > ScheduledDate && !IsCompleted;
        
        public int DaysOverdue => IsOverdue ? (DateTime.UtcNow - ScheduledDate).Days : 0;
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
    
    public class ReviewSession
    {
        [Key]
        public int SessionId { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        
        public DateTime? EndTime { get; set; }
        
        public int ItemsReviewed { get; set; } = 0;
        
        public int CorrectAnswers { get; set; } = 0;
        
        public int IncorrectAnswers { get; set; } = 0;
        
        public double AccuracyRate => ItemsReviewed > 0 ? (double)CorrectAnswers / ItemsReviewed * 100 : 0;
        
        public TimeSpan Duration => (EndTime ?? DateTime.UtcNow) - StartTime;
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
        
        public virtual ICollection<ReviewResult> Results { get; set; } = new List<ReviewResult>();
    }
    
    public class ReviewResult
    {
        [Key]
        public int ResultId { get; set; }
        
        [Required]
        public int SessionId { get; set; }
        
        [Required]
        public int QueueId { get; set; }
        
        public bool IsCorrect { get; set; }
        
        public int ResponseTimeMs { get; set; }
        
        [MaxLength(100)]
        public string UserAnswer { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string CorrectAnswer { get; set; } = string.Empty;
        
        public DateTime ReviewedAt { get; set; } = DateTime.UtcNow;
        
        public int DifficultyRating { get; set; } = 0; // 0 = Easy, 1 = Normal, 2 = Hard
        
        // Navigation properties
        [ForeignKey("SessionId")]
        public virtual ReviewSession Session { get; set; } = null!;
        
        [ForeignKey("QueueId")]
        public virtual ReviewQueue QueueItem { get; set; } = null!;
    }
}