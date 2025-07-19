using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace JapaneseTracker.Models
{
    public class Grammar
    {
        [Key]
        public int GrammarId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Pattern { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(300)]
        public string Meaning { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string Structure { get; set; } = string.Empty;
        
        [MaxLength(10)]
        public string JLPTLevel { get; set; } = string.Empty;
        
        [MaxLength(2000)]
        public string ExamplesJson { get; set; } = "[]";
        
        [MaxLength(1000)]
        public string Notes { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string RelatedGrammarJson { get; set; } = "[]";
        
        // Navigation properties
        public virtual ICollection<GrammarProgress> ProgressRecords { get; set; } = new List<GrammarProgress>();
        
        // Helper properties for JSON serialization
        public List<GrammarExample> Examples
        {
            get => JsonSerializer.Deserialize<List<GrammarExample>>(ExamplesJson) ?? new List<GrammarExample>();
            set => ExamplesJson = JsonSerializer.Serialize(value);
        }
        
        public List<string> RelatedGrammar
        {
            get => JsonSerializer.Deserialize<List<string>>(RelatedGrammarJson) ?? new List<string>();
            set => RelatedGrammarJson = JsonSerializer.Serialize(value);
        }
    }
    
    public class GrammarExample
    {
        public string Japanese { get; set; } = string.Empty;
        public string Reading { get; set; } = string.Empty;
        public string English { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
    }
}
