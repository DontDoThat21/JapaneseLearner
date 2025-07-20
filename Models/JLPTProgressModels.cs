using System;
using System.Collections.Generic;

namespace JapaneseTracker.Models
{
    public class StudyItem
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Meaning { get; set; } = string.Empty;
        public string Reading { get; set; } = string.Empty;
        public string JLPTLevel { get; set; } = string.Empty;
        public int SRSLevel { get; set; }
        public DateTime? LastReviewed { get; set; }
        public bool IsWeak { get; set; }
        public double AccuracyRate { get; set; }
        public int ReviewCount { get; set; }
        public string Category { get; set; } = string.Empty; // Kanji, Vocabulary, Grammar
    }

    public class ExamReadiness
    {
        public string Level { get; set; } = string.Empty;
        public double ReadinessScore { get; set; }
        public string ReadinessLevel { get; set; } = string.Empty;
        public TimeSpan EstimatedTimeToReady { get; set; }
        public List<string> Feedback { get; set; } = new();
        public bool IsReady { get; set; }
        public DateTime NextExamDate { get; set; }
        public List<string> StrengthAreas { get; set; } = new();
        public List<string> WeakAreas { get; set; } = new();
    }

    public class StudyStreak
    {
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public DateTime LastStudyDate { get; set; }
        public List<DateTime> StudyDates { get; set; } = new();
        public double AverageSessionLength { get; set; }
        public int TotalStudyDays { get; set; }
    }

    public class JLPTProgressDetailedView
    {
        public string Level { get; set; } = string.Empty;
        public List<StudyItem> LearnedItems { get; set; } = new();
        public List<StudyItem> WeakItems { get; set; } = new();
        public List<StudyItem> UnlearnedItems { get; set; } = new();
        public Dictionary<string, double> CategoryProgress { get; set; } = new();
        public List<string> NextSteps { get; set; } = new();
        public DateTime LastUpdated { get; set; }
    }

    public class MotivationalContent
    {
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // Encouragement, Achievement, Tip
        public bool IsVisible { get; set; }
        public string IconName { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }

    public class StudyPlanRecommendation
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int Priority { get; set; }
        public TimeSpan EstimatedTime { get; set; }
        public bool IsCompleted { get; set; }
        public string ActionType { get; set; } = string.Empty; // Practice, Review, Learn
    }
}