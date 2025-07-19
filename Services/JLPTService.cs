using System;
using System.Collections.Generic;
using System.Linq;
using JapaneseTracker.Models;

namespace JapaneseTracker.Services
{
    public class JLPTService
    {
        private readonly Dictionary<string, JLPTLevelInfo> _jlptLevels;
        
        public JLPTService()
        {
            _jlptLevels = new Dictionary<string, JLPTLevelInfo>
            {
                ["N5"] = new JLPTLevelInfo
                {
                    Level = "N5",
                    Name = "Basic",
                    Description = "Basic understanding of Japanese",
                    RequiredKanji = 100,
                    RequiredVocabulary = 800,
                    RequiredGrammar = 50,
                    Color = "#4CAF50",
                    TestSections = new[] { "Language Knowledge (Vocabulary/Grammar)", "Reading", "Listening" }
                },
                ["N4"] = new JLPTLevelInfo
                {
                    Level = "N4",
                    Name = "Elementary",
                    Description = "Elementary understanding of Japanese",
                    RequiredKanji = 300,
                    RequiredVocabulary = 1500,
                    RequiredGrammar = 80,
                    Color = "#8BC34A",
                    TestSections = new[] { "Language Knowledge (Vocabulary/Grammar)", "Reading", "Listening" }
                },
                ["N3"] = new JLPTLevelInfo
                {
                    Level = "N3",
                    Name = "Intermediate",
                    Description = "Intermediate understanding of Japanese",
                    RequiredKanji = 650,
                    RequiredVocabulary = 3750,
                    RequiredGrammar = 200,
                    Color = "#FFC107",
                    TestSections = new[] { "Language Knowledge (Vocabulary)", "Language Knowledge (Grammar)/Reading", "Listening" }
                },
                ["N2"] = new JLPTLevelInfo
                {
                    Level = "N2",
                    Name = "Upper Intermediate",
                    Description = "Upper intermediate understanding of Japanese",
                    RequiredKanji = 1000,
                    RequiredVocabulary = 6000,
                    RequiredGrammar = 400,
                    Color = "#FF9800",
                    TestSections = new[] { "Language Knowledge (Vocabulary)", "Language Knowledge (Grammar)/Reading", "Listening" }
                },
                ["N1"] = new JLPTLevelInfo
                {
                    Level = "N1",
                    Name = "Advanced",
                    Description = "Advanced understanding of Japanese",
                    RequiredKanji = 2000,
                    RequiredVocabulary = 10000,
                    RequiredGrammar = 800,
                    Color = "#F44336",
                    TestSections = new[] { "Language Knowledge (Vocabulary)", "Language Knowledge (Grammar)/Reading", "Listening" }
                }
            };
        }
        
        public JLPTLevelInfo GetLevelInfo(string level)
        {
            return _jlptLevels.ContainsKey(level) ? _jlptLevels[level] : _jlptLevels["N5"];
        }
        
        public List<JLPTLevelInfo> GetAllLevels()
        {
            return _jlptLevels.Values.OrderByDescending(l => l.Level).ToList();
        }
        
        public string GetNextLevel(string currentLevel)
        {
            return currentLevel switch
            {
                "N5" => "N4",
                "N4" => "N3",
                "N3" => "N2",
                "N2" => "N1",
                "N1" => "N1", // Already at highest level
                _ => "N5"
            };
        }
        
        public string GetPreviousLevel(string currentLevel)
        {
            return currentLevel switch
            {
                "N1" => "N2",
                "N2" => "N3",
                "N3" => "N4",
                "N4" => "N5",
                "N5" => "N5", // Already at lowest level
                _ => "N5"
            };
        }
        
        public JLPTProgress CalculateProgress(string level, int kanjiLearned, int vocabularyLearned, int grammarLearned)
        {
            var levelInfo = GetLevelInfo(level);
            
            var kanjiProgress = levelInfo.RequiredKanji > 0 ? 
                Math.Min(100, (double)kanjiLearned / levelInfo.RequiredKanji * 100) : 100;
            
            var vocabularyProgress = levelInfo.RequiredVocabulary > 0 ? 
                Math.Min(100, (double)vocabularyLearned / levelInfo.RequiredVocabulary * 100) : 100;
            
            var grammarProgress = levelInfo.RequiredGrammar > 0 ? 
                Math.Min(100, (double)grammarLearned / levelInfo.RequiredGrammar * 100) : 100;
            
            var overallProgress = (kanjiProgress + vocabularyProgress + grammarProgress) / 3;
            
            return new JLPTProgress
            {
                Level = level,
                KanjiProgress = kanjiProgress,
                VocabularyProgress = vocabularyProgress,
                GrammarProgress = grammarProgress,
                OverallProgress = overallProgress,
                KanjiLearned = kanjiLearned,
                VocabularyLearned = vocabularyLearned,
                GrammarLearned = grammarLearned,
                RequiredKanji = levelInfo.RequiredKanji,
                RequiredVocabulary = levelInfo.RequiredVocabulary,
                RequiredGrammar = levelInfo.RequiredGrammar,
                IsCompleted = overallProgress >= 90
            };
        }
        
        public List<string> GetRecommendedStudyPlan(string level)
        {
            return level switch
            {
                "N5" => new List<string>
                {
                    "Master Hiragana and Katakana",
                    "Learn basic kanji (100 characters)",
                    "Study essential vocabulary (800 words)",
                    "Practice basic grammar patterns",
                    "Read simple texts and manga",
                    "Listen to beginner audio materials"
                },
                "N4" => new List<string>
                {
                    "Expand kanji knowledge (300 characters)",
                    "Build vocabulary (1500 words)",
                    "Study intermediate grammar",
                    "Practice reading short articles",
                    "Listen to slow-paced conversations",
                    "Start writing simple sentences"
                },
                "N3" => new List<string>
                {
                    "Learn more complex kanji (650 characters)",
                    "Expand vocabulary significantly (3750 words)",
                    "Master intermediate grammar patterns",
                    "Read news articles and short stories",
                    "Listen to normal-speed conversations",
                    "Practice writing paragraphs"
                },
                "N2" => new List<string>
                {
                    "Study advanced kanji (1000 characters)",
                    "Master extensive vocabulary (6000 words)",
                    "Learn complex grammar structures",
                    "Read newspapers and novels",
                    "Listen to news and documentaries",
                    "Practice formal writing"
                },
                "N1" => new List<string>
                {
                    "Master all common kanji (2000+ characters)",
                    "Achieve native-level vocabulary (10000+ words)",
                    "Understand nuanced grammar",
                    "Read academic and technical texts",
                    "Understand rapid natural speech",
                    "Write formal reports and essays"
                },
                _ => new List<string> { "Start with N5 level studies" }
            };
        }
        
        public TimeSpan GetEstimatedStudyTime(string fromLevel, string toLevel)
        {
            var levelOrder = new[] { "N5", "N4", "N3", "N2", "N1" };
            var fromIndex = Array.IndexOf(levelOrder, fromLevel);
            var toIndex = Array.IndexOf(levelOrder, toLevel);
            
            if (fromIndex == -1 || toIndex == -1 || fromIndex >= toIndex)
            {
                return TimeSpan.Zero;
            }
            
            var studyHours = 0;
            for (int i = fromIndex; i < toIndex; i++)
            {
                studyHours += levelOrder[i] switch
                {
                    "N5" => 150,  // Hours to complete N5
                    "N4" => 300,  // Hours to complete N4
                    "N3" => 450,  // Hours to complete N3
                    "N2" => 600,  // Hours to complete N2
                    "N1" => 900,  // Hours to complete N1
                    _ => 0
                };
            }
            
            return TimeSpan.FromHours(studyHours);
        }
    }
    
    public class JLPTLevelInfo
    {
        public string Level { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int RequiredKanji { get; set; }
        public int RequiredVocabulary { get; set; }
        public int RequiredGrammar { get; set; }
        public string Color { get; set; } = string.Empty;
        public string[] TestSections { get; set; } = Array.Empty<string>();
    }
    
    public class JLPTProgress
    {
        public string Level { get; set; } = string.Empty;
        public double KanjiProgress { get; set; }
        public double VocabularyProgress { get; set; }
        public double GrammarProgress { get; set; }
        public double OverallProgress { get; set; }
        public int KanjiLearned { get; set; }
        public int VocabularyLearned { get; set; }
        public int GrammarLearned { get; set; }
        public int RequiredKanji { get; set; }
        public int RequiredVocabulary { get; set; }
        public int RequiredGrammar { get; set; }
        public bool IsCompleted { get; set; }
    }
}
