using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JapaneseTracker.Models
{
    public class GrammarProgress
    {
        [Key]
        public int ProgressId { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public int GrammarId { get; set; }
        
        [Range(0, 100)]
        public int UnderstandingLevel { get; set; } = 0;
        
        public int PracticeCount { get; set; } = 0;
        
        public DateTime LastPracticed { get; set; } = DateTime.UtcNow;
        
        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
        
        [ForeignKey("GrammarId")]
        public virtual Grammar Grammar { get; set; } = null!;
        
        // Calculated properties
        public bool IsMastered => UnderstandingLevel >= 80;
        
        public string MasteryLevel => UnderstandingLevel switch
        {
            >= 80 => "Mastered",
            >= 60 => "Good",
            >= 40 => "Fair",
            >= 20 => "Poor",
            _ => "Beginner"
        };
    }
}
