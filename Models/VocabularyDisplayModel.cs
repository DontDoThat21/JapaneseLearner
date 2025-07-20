using System;
using System.Collections.Generic;

namespace JapaneseTracker.Models
{
    /// <summary>
    /// Combined view model for displaying vocabulary with progress data
    /// </summary>
    public class VocabularyDisplayModel
    {
        public Vocabulary Vocabulary { get; set; } = null!;
        public VocabularyProgress? Progress { get; set; }
        
        // Vocabulary properties (delegates to Vocabulary object)
        public int VocabId => Vocabulary.VocabId;
        public string Word => Vocabulary.Word;
        public string Reading => Vocabulary.Reading;
        public string Furigana => Vocabulary.Reading; // Alias for XAML compatibility
        public string Meaning => Vocabulary.Meaning;
        public string PartOfSpeech => Vocabulary.PartOfSpeech;
        public string JLPTLevel => Vocabulary.JLPTLevel;
        public int PitchAccent => Vocabulary.PitchAccent;
        public List<ExampleSentence> ExampleSentences => Vocabulary.ExampleSentences;
        public List<string> RelatedKanji => Vocabulary.RelatedKanji;
        public string Notes => ""; // TODO: Add notes field if needed
        
        // Progress properties (delegates to Progress object or provides defaults)
        public int SRSLevel => Progress?.SRSLevel ?? 0;
        public double AccuracyRate => Progress?.AccuracyRate ?? 0.0;
        public DateTime NextReviewDate => Progress?.NextReviewDate ?? DateTime.Now;
        public bool IsReviewDue => Progress?.IsReviewDue ?? false;
        public int CorrectCount => Progress?.CorrectCount ?? 0;
        public int IncorrectCount => Progress?.IncorrectCount ?? 0;
        public int TotalReviews => Progress?.TotalReviews ?? 0;
        
        // Computed properties for UI
        public string MasteryLevel
        {
            get
            {
                if (Progress == null) return "New";
                
                return SRSLevel switch
                {
                    0 => "New",
                    1 => "Learning",
                    2 => "Apprentice I",
                    3 => "Apprentice II", 
                    4 => "Guru I",
                    5 => "Guru II",
                    6 => "Master",
                    7 => "Enlightened",
                    8 => "Burned",
                    _ => "Unknown"
                };
            }
        }
        
        public string ProgressDescription
        {
            get
            {
                if (Progress == null) return "Not studied yet";
                
                if (IsReviewDue)
                {
                    return $"Review due • {TotalReviews} reviews • {AccuracyRate:F0}% accuracy";
                }
                else
                {
                    var timeUntilReview = NextReviewDate - DateTime.UtcNow;
                    if (timeUntilReview.TotalDays > 1)
                        return $"Review in {timeUntilReview.Days} days • {TotalReviews} reviews";
                    else if (timeUntilReview.TotalHours > 1)
                        return $"Review in {timeUntilReview.Hours} hours • {TotalReviews} reviews";
                    else
                        return $"Review soon • {TotalReviews} reviews";
                }
            }
        }
    }
}