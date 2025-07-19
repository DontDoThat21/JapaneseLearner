using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace JapaneseTracker.Models
{
    public class Kanji
    {
        [Key]
        public int KanjiId { get; set; }
        
        [Required]
        [MaxLength(1)]
        public string Character { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(200)]
        public string Meaning { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string OnReadingsJson { get; set; } = "[]";
        
        [MaxLength(500)]
        public string KunReadingsJson { get; set; } = "[]";
        
        public int StrokeCount { get; set; }
        
        [MaxLength(10)]
        public string JLPTLevel { get; set; } = string.Empty;
        
        public int Grade { get; set; }
        
        [MaxLength(1000)]
        public string RadicalsJson { get; set; } = "[]";
        
        [MaxLength(2000)]
        public string ExampleWordsJson { get; set; } = "[]";
        
        // Navigation properties
        public virtual ICollection<KanjiProgress> ProgressRecords { get; set; } = new List<KanjiProgress>();
        
        // Helper properties for JSON serialization
        public List<string> OnReadings
        {
            get => JsonSerializer.Deserialize<List<string>>(OnReadingsJson) ?? new List<string>();
            set => OnReadingsJson = JsonSerializer.Serialize(value);
        }
        
        public List<string> KunReadings
        {
            get => JsonSerializer.Deserialize<List<string>>(KunReadingsJson) ?? new List<string>();
            set => KunReadingsJson = JsonSerializer.Serialize(value);
        }
        
        public List<string> Radicals
        {
            get => JsonSerializer.Deserialize<List<string>>(RadicalsJson) ?? new List<string>();
            set => RadicalsJson = JsonSerializer.Serialize(value);
        }
        
        public List<VocabularyExample> ExampleWords
        {
            get => JsonSerializer.Deserialize<List<VocabularyExample>>(ExampleWordsJson) ?? new List<VocabularyExample>();
            set => ExampleWordsJson = JsonSerializer.Serialize(value);
        }
    }
    
    public class VocabularyExample
    {
        public string Word { get; set; } = string.Empty;
        public string Reading { get; set; } = string.Empty;
        public string Meaning { get; set; } = string.Empty;
    }
}
