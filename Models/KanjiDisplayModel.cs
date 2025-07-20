using System;
using System.Collections.Generic;

namespace JapaneseTracker.Models
{
    public class KanjiDisplayModel
    {
        public KanjiDisplayModel(Kanji kanji, KanjiProgress? progress = null)
        {
            Kanji = kanji;
            Progress = progress;
        }

        public Kanji Kanji { get; set; }
        public KanjiProgress? Progress { get; set; }

        // Delegate properties from Kanji for easier binding
        public int KanjiId => Kanji.KanjiId;
        public string Character => Kanji.Character;
        public string Meaning => Kanji.Meaning;
        public List<string> OnReadings => Kanji.OnReadings;
        public List<string> KunReadings => Kanji.KunReadings;
        public int StrokeCount => Kanji.StrokeCount;
        public string JLPTLevel => Kanji.JLPTLevel;
        public int Grade => Kanji.Grade;
        public List<string> Radicals => Kanji.Radicals;
        public List<VocabularyExample> ExampleWords => Kanji.ExampleWords;

        // Progress-related properties
        public int SRSLevel => Progress?.SRSLevel ?? 0;
        public bool IsReviewDue => Progress?.IsReviewDue ?? false;
        public double AccuracyRate => Progress?.AccuracyRate ?? 0;
        public DateTime NextReviewDate => Progress?.NextReviewDate ?? DateTime.UtcNow;
        public int CorrectCount => Progress?.CorrectCount ?? 0;
        public int IncorrectCount => Progress?.IncorrectCount ?? 0;
        public DateTime LastReviewed => Progress?.LastReviewed ?? DateTime.MinValue;
    }
}