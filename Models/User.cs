using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JapaneseTracker.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;
        
        [MaxLength(10)]
        public string CurrentJLPTLevel { get; set; } = "N5";
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime LastActive { get; set; } = DateTime.UtcNow;
        
        public int StudyStreak { get; set; } = 0;
        
        // Navigation properties
        public virtual ICollection<KanjiProgress> KanjiProgress { get; set; } = new List<KanjiProgress>();
        public virtual ICollection<VocabularyProgress> VocabularyProgress { get; set; } = new List<VocabularyProgress>();
        public virtual ICollection<GrammarProgress> GrammarProgress { get; set; } = new List<GrammarProgress>();
        public virtual ICollection<StudySession> StudySessions { get; set; } = new List<StudySession>();
        public virtual ICollection<KanaProgress> KanaProgress { get; set; } = new List<KanaProgress>();
    }
}
