using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JapaneseTracker.Models
{
    public class KanjiProgress
    {
        [Key]
        public int ProgressId { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public int KanjiId { get; set; }
        
        public int SRSLevel { get; set; } = 0;
        
        public DateTime NextReviewDate { get; set; } = DateTime.UtcNow;
        
        public int CorrectCount { get; set; } = 0;
        
        public int IncorrectCount { get; set; } = 0;
        
        public DateTime LastReviewed { get; set; } = DateTime.UtcNow;
        
        [Range(0, 100)]
        public int WritingMastery { get; set; } = 0;
        
        [Range(0, 100)]
        public int ReadingMastery { get; set; } = 0;
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
        
        [ForeignKey("KanjiId")]
        public virtual Kanji Kanji { get; set; } = null!;
        
        // Calculated properties
        public double AccuracyRate => 
            (CorrectCount + IncorrectCount) > 0 ? 
                (double)CorrectCount / (CorrectCount + IncorrectCount) * 100 : 0;
        
        public bool IsReviewDue => NextReviewDate <= DateTime.UtcNow;
        
        public int TotalReviews => CorrectCount + IncorrectCount;
    }
}
