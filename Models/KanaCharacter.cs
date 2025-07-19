using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JapaneseTracker.Models
{
    public class KanaCharacter
    {
        [Key]
        public int KanaId { get; set; }
        
        [Required]
        [MaxLength(1)]
        public string Character { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(10)]
        public string Type { get; set; } = string.Empty; // Hiragana, Katakana
        
        [Required]
        [MaxLength(10)]
        public string Romaji { get; set; } = string.Empty;
        
        public int StrokeCount { get; set; } = 0;
        
        // Navigation properties
        public virtual ICollection<KanaProgress> ProgressRecords { get; set; } = new List<KanaProgress>();
    }
    
    public class KanaProgress
    {
        [Key]
        public int ProgressId { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public int KanaId { get; set; }
        
        public bool Mastered { get; set; } = false;
        
        public int PracticeCount { get; set; } = 0;
        
        public DateTime LastPracticed { get; set; } = DateTime.UtcNow;
        
        public int CorrectCount { get; set; } = 0;
        
        public int IncorrectCount { get; set; } = 0;
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
        
        [ForeignKey("KanaId")]
        public virtual KanaCharacter KanaCharacter { get; set; } = null!;
        
        // Calculated properties
        public double AccuracyRate => 
            (CorrectCount + IncorrectCount) > 0 ? 
                (double)CorrectCount / (CorrectCount + IncorrectCount) * 100 : 0;
        
        public int TotalAttempts => CorrectCount + IncorrectCount;
    }
}
