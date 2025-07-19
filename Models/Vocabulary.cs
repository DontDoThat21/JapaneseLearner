using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace JapaneseTracker.Models
{
    public class Vocabulary
    {
        [Key]
        public int VocabId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Word { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string Reading { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(300)]
        public string Meaning { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string PartOfSpeech { get; set; } = string.Empty;
        
        [MaxLength(10)]
        public string JLPTLevel { get; set; } = string.Empty;
        
        public int PitchAccent { get; set; } = 0;
        
        [MaxLength(2000)]
        public string ExampleSentencesJson { get; set; } = "[]";
        
        [MaxLength(500)]
        public string RelatedKanjiJson { get; set; } = "[]";
        
        // Navigation properties
        public virtual ICollection<VocabularyProgress> ProgressRecords { get; set; } = new List<VocabularyProgress>();
        
        // Helper properties for JSON serialization
        public List<ExampleSentence> ExampleSentences
        {
            get => JsonSerializer.Deserialize<List<ExampleSentence>>(ExampleSentencesJson) ?? new List<ExampleSentence>();
            set => ExampleSentencesJson = JsonSerializer.Serialize(value);
        }
        
        public List<string> RelatedKanji
        {
            get => JsonSerializer.Deserialize<List<string>>(RelatedKanjiJson) ?? new List<string>();
            set => RelatedKanjiJson = JsonSerializer.Serialize(value);
        }
    }
    
    public class ExampleSentence
    {
        public string Japanese { get; set; } = string.Empty;
        public string Reading { get; set; } = string.Empty;
        public string English { get; set; } = string.Empty;
    }
}
