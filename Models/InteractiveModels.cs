using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JapaneseTracker.Models
{
    public class SentenceBuildingExercise
    {
        [Key]
        public int ExerciseId { get; set; }
        
        [Required]
        [MaxLength(500)]
        public string TargetSentence { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(500)]
        public string TargetReading { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(500)]
        public string EnglishTranslation { get; set; } = string.Empty;
        
        [MaxLength(10)]
        public string JLPTLevel { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string GrammarPattern { get; set; } = string.Empty;
        
        public int Difficulty { get; set; } = 1; // 1-5 scale
        
        [MaxLength(1000)]
        public string WordBankJson { get; set; } = "[]"; // JSON array of words/particles
        
        [MaxLength(2000)]
        public string HintsJson { get; set; } = "[]"; // JSON array of hints
        
        public List<string> WordBank
        {
            get => System.Text.Json.JsonSerializer.Deserialize<List<string>>(WordBankJson) ?? new List<string>();
            set => WordBankJson = System.Text.Json.JsonSerializer.Serialize(value);
        }
        
        public List<string> Hints
        {
            get => System.Text.Json.JsonSerializer.Deserialize<List<string>>(HintsJson) ?? new List<string>();
            set => HintsJson = System.Text.Json.JsonSerializer.Serialize(value);
        }
    }
    
    public class WritingPracticeStroke
    {
        [Key]
        public int StrokeId { get; set; }
        
        [Required]
        public int PracticeSessionId { get; set; }
        
        public double StartX { get; set; }
        public double StartY { get; set; }
        public double EndX { get; set; }
        public double EndY { get; set; }
        
        public DateTime StrokeTime { get; set; } = DateTime.UtcNow;
        
        public int StrokeOrder { get; set; }
        
        [MaxLength(1000)]
        public string StrokeDataJson { get; set; } = "[]"; // Full stroke path data
    }
    
    public class WritingPracticeSession
    {
        [Key]
        public int SessionId { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        [MaxLength(1)]
        public string Character { get; set; } = string.Empty; // Kanji or Kana being practiced
        
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        
        public DateTime? EndTime { get; set; }
        
        public int AccuracyScore { get; set; } = 0; // 0-100
        
        public bool IsCompleted { get; set; } = false;
        
        public virtual ICollection<WritingPracticeStroke> Strokes { get; set; } = new List<WritingPracticeStroke>();
    }
    
    public class PitchAccentPattern
    {
        [Key]
        public int PatternId { get; set; }
        
        [Required]
        public int VocabularyId { get; set; }
        
        [MaxLength(50)]
        public string PatternType { get; set; } = string.Empty; // HeiBan, AtaMa, NaKaDaka, OdAka
        
        public int AccentPosition { get; set; } = 0; // 0 for heiban, position for others
        
        [MaxLength(100)]
        public string VisualizationData { get; set; } = string.Empty; // Pattern visualization info
        
        public virtual Vocabulary Vocabulary { get; set; } = null!;
    }
}