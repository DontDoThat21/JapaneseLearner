using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JapaneseTracker.Models
{
    public class StudySession
    {
        [Key]
        public int SessionId { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string SessionType { get; set; } = string.Empty; // Kanji, Vocabulary, Grammar, Mixed
        
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        
        public DateTime EndTime { get; set; } = DateTime.UtcNow;
        
        public int ItemsStudied { get; set; } = 0;
        
        public int CorrectAnswers { get; set; } = 0;
        
        public int IncorrectAnswers { get; set; } = 0;
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
        
        // Calculated properties
        public TimeSpan Duration => EndTime - StartTime;
        
        public double AccuracyRate => 
            (CorrectAnswers + IncorrectAnswers) > 0 ? 
                (double)CorrectAnswers / (CorrectAnswers + IncorrectAnswers) * 100 : 0;
        
        public int TotalAnswers => CorrectAnswers + IncorrectAnswers;
        
        public bool IsCompleted => EndTime > StartTime;
    }
}
