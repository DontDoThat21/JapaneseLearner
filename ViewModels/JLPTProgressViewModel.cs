using System;
using System.Collections.Generic;
using JapaneseTracker.Services;
using JapaneseTracker.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using JapaneseTracker.Commands;

namespace JapaneseTracker.ViewModels
{
    public class JLPTProgressViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly JLPTService _jlptService;
        
        private ObservableCollection<JLPTProgress> _jlptProgressList = new();
        private JLPTProgress? _selectedProgress;
        private string _currentLevel = "N5";
        private bool _isLoading = false;
        private User? _currentUser;
        
        // Enhanced properties for detailed views
        private ObservableCollection<StudyItem> _weakKanjiItems = new();
        private ObservableCollection<StudyItem> _weakVocabularyItems = new();
        private ObservableCollection<StudyItem> _weakGrammarItems = new();
        private ObservableCollection<string> _studyRecommendations = new();
        private ExamReadiness _examReadiness = new();
        private ObservableCollection<StudySession> _recentStudySessions = new();
        private int _currentStreak = 0;
        private bool _showDetailedView = false;
        private string _selectedCategory = "Kanji";
        
        public JLPTProgressViewModel(
            DatabaseService databaseService,
            JLPTService jlptService)
        {
            _databaseService = databaseService;
            _jlptService = jlptService;
            
            // Commands
            LoadProgressCommand = new RelayCommand(async () => await LoadProgressAsync());
            SelectLevelCommand = new RelayCommand<string>(SelectLevel);
            ToggleDetailedViewCommand = new RelayCommand(() => ShowDetailedView = !ShowDetailedView);
            SelectCategoryCommand = new RelayCommand<string>(SelectCategory);
            StartPracticeCommand = new RelayCommand<string>(StartPractice);
            ReviewWeakAreasCommand = new RelayCommand(ReviewWeakAreas);
            
            JLPTLevels = new ObservableCollection<string> { "N5", "N4", "N3", "N2", "N1" };
            Categories = new ObservableCollection<string> { "Kanji", "Vocabulary", "Grammar" };
            
            // Initialize
            _ = InitializeAsync();
        }
        
        public ObservableCollection<JLPTProgress> JLPTProgressList
        {
            get => _jlptProgressList;
            set => SetProperty(ref _jlptProgressList, value);
        }
        
        public JLPTProgress? SelectedProgress
        {
            get => _selectedProgress;
            set => SetProperty(ref _selectedProgress, value);
        }
        
        public string CurrentLevel
        {
            get => _currentLevel;
            set => SetProperty(ref _currentLevel, value);
        }
        
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }
        
        public User? CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }
        
        public ObservableCollection<string> JLPTLevels { get; }
        public ObservableCollection<string> Categories { get; }
        
        public ICommand LoadProgressCommand { get; }
        public ICommand SelectLevelCommand { get; }
        public ICommand ToggleDetailedViewCommand { get; }
        public ICommand SelectCategoryCommand { get; }
        public ICommand StartPracticeCommand { get; }
        public ICommand ReviewWeakAreasCommand { get; }
        
        // Enhanced properties
        public ObservableCollection<StudyItem> WeakKanjiItems
        {
            get => _weakKanjiItems;
            set => SetProperty(ref _weakKanjiItems, value);
        }
        
        public ObservableCollection<StudyItem> WeakVocabularyItems
        {
            get => _weakVocabularyItems;
            set => SetProperty(ref _weakVocabularyItems, value);
        }
        
        public ObservableCollection<StudyItem> WeakGrammarItems
        {
            get => _weakGrammarItems;
            set => SetProperty(ref _weakGrammarItems, value);
        }
        
        public ObservableCollection<string> StudyRecommendations
        {
            get => _studyRecommendations;
            set => SetProperty(ref _studyRecommendations, value);
        }
        
        public ExamReadiness ExamReadiness
        {
            get => _examReadiness;
            set => SetProperty(ref _examReadiness, value);
        }
        
        public ObservableCollection<StudySession> RecentStudySessions
        {
            get => _recentStudySessions;
            set => SetProperty(ref _recentStudySessions, value);
        }
        
        public int CurrentStreak
        {
            get => _currentStreak;
            set => SetProperty(ref _currentStreak, value);
        }
        
        public bool ShowDetailedView
        {
            get => _showDetailedView;
            set => SetProperty(ref _showDetailedView, value);
        }
        
        public string SelectedCategory
        {
            get => _selectedCategory;
            set => SetProperty(ref _selectedCategory, value);
        }
        
        public ObservableCollection<StudyItem> CurrentCategoryItems
        {
            get
            {
                return SelectedCategory switch
                {
                    "Kanji" => WeakKanjiItems,
                    "Vocabulary" => WeakVocabularyItems,
                    "Grammar" => WeakGrammarItems,
                    _ => WeakKanjiItems
                };
            }
        }
        
        private async Task InitializeAsync()
        {
            CurrentUser = await _databaseService.GetUserByUsernameAsync("DefaultUser");
            if (CurrentUser != null)
            {
                CurrentLevel = CurrentUser.CurrentJLPTLevel;
                await LoadProgressAsync();
            }
        }
        
        private async Task LoadProgressAsync()
        {
            if (CurrentUser == null) return;
            
            IsLoading = true;
            try
            {
                var progressList = new List<JLPTProgress>();
                
                foreach (var level in JLPTLevels)
                {
                    // Get statistics for this level
                    var stats = await _databaseService.GetStudyStatisticsAsync(CurrentUser.UserId);
                    
                    // Calculate progress (simplified - in real app you'd have level-specific data)
                    var kanjiLearned = stats.ContainsKey("LearnedKanji") ? stats["LearnedKanji"] : 0;
                    var vocabularyLearned = stats.ContainsKey("LearnedVocabulary") ? stats["LearnedVocabulary"] : 0;
                    var grammarLearned = stats.ContainsKey("LearnedGrammar") ? stats["LearnedGrammar"] : 0;
                    
                    var progress = _jlptService.CalculateProgress(level, kanjiLearned, vocabularyLearned, grammarLearned);
                    progressList.Add(progress);
                }
                
                JLPTProgressList = new ObservableCollection<JLPTProgress>(progressList);
                SelectedProgress = JLPTProgressList.FirstOrDefault(p => p.Level == CurrentLevel);
                
                // Load additional data
                await LoadDetailedDataAsync();
                await LoadStudyRecommendationsAsync();
                await LoadExamReadinessAsync();
                await LoadRecentStudySessionsAsync();
                await LoadCurrentStreakAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading JLPT progress: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private void SelectLevel(string? level)
        {
            if (!string.IsNullOrEmpty(level))
            {
                CurrentLevel = level;
                SelectedProgress = JLPTProgressList.FirstOrDefault(p => p.Level == level);
                OnPropertyChanged(nameof(CurrentCategoryItems));
            }
        }
        
        private void SelectCategory(string? category)
        {
            if (!string.IsNullOrEmpty(category))
            {
                SelectedCategory = category;
                OnPropertyChanged(nameof(CurrentCategoryItems));
            }
        }
        
        private void StartPractice(string? category)
        {
            // This would typically navigate to practice view
            System.Diagnostics.Debug.WriteLine($"Starting practice for {category} at {CurrentLevel} level");
        }
        
        private void ReviewWeakAreas()
        {
            // This would typically navigate to review view with weak items
            System.Diagnostics.Debug.WriteLine("Starting review of weak areas");
        }
        
        private async Task LoadDetailedDataAsync()
        {
            if (CurrentUser == null) return;
            
            try
            {
                // Load weak kanji items (simplified - would filter by JLPT level and SRS level)
                var kanjiProgress = await _databaseService.GetKanjiProgressByUserAsync(CurrentUser.UserId);
                var weakKanji = kanjiProgress
                    .Where(kp => kp.SRSLevel <= 2 && kp.Kanji != null) // Low SRS level indicates weakness
                    .Take(10)
                    .Select(kp => new StudyItem
                    {
                        Id = kp.KanjiId,
                        Content = kp.Kanji!.Character,
                        Meaning = kp.Kanji.Meaning,
                        Reading = kp.Kanji.OnReading + ", " + kp.Kanji.KunReading,
                        JLPTLevel = kp.Kanji.JLPTLevel,
                        SRSLevel = kp.SRSLevel,
                        LastReviewed = kp.LastReviewedDate,
                        IsWeak = kp.SRSLevel <= 2
                    })
                    .ToList();
                
                WeakKanjiItems = new ObservableCollection<StudyItem>(weakKanji);
                
                // Load weak vocabulary items
                var vocabularyProgress = await _databaseService.GetVocabularyProgressByUserAsync(CurrentUser.UserId);
                var weakVocabulary = vocabularyProgress
                    .Where(vp => vp.SRSLevel <= 2 && vp.Vocabulary != null)
                    .Take(10)
                    .Select(vp => new StudyItem
                    {
                        Id = vp.VocabId,
                        Content = vp.Vocabulary!.Word,
                        Meaning = vp.Vocabulary.Meaning,
                        Reading = vp.Vocabulary.Furigana,
                        JLPTLevel = vp.Vocabulary.JLPTLevel,
                        SRSLevel = vp.SRSLevel,
                        LastReviewed = vp.LastReviewedDate,
                        IsWeak = vp.SRSLevel <= 2
                    })
                    .ToList();
                
                WeakVocabularyItems = new ObservableCollection<StudyItem>(weakVocabulary);
                
                // Load weak grammar items
                var grammarProgress = await _databaseService.GetGrammarProgressByUserAsync(CurrentUser.UserId);
                var weakGrammar = grammarProgress
                    .Where(gp => gp.UnderstandingLevel <= 2 && gp.Grammar != null)
                    .Take(10)
                    .Select(gp => new StudyItem
                    {
                        Id = gp.GrammarId,
                        Content = gp.Grammar!.Pattern,
                        Meaning = gp.Grammar.Meaning,
                        Reading = gp.Grammar.Usage,
                        JLPTLevel = gp.Grammar.JLPTLevel,
                        SRSLevel = gp.UnderstandingLevel,
                        LastReviewed = gp.LastReviewedDate,
                        IsWeak = gp.UnderstandingLevel <= 2
                    })
                    .ToList();
                
                WeakGrammarItems = new ObservableCollection<StudyItem>(weakGrammar);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading detailed data: {ex.Message}");
            }
        }
        
        private async Task LoadStudyRecommendationsAsync()
        {
            try
            {
                var recommendations = GetStudyRecommendations(CurrentLevel);
                
                // Add personalized recommendations based on weak areas
                if (SelectedProgress != null)
                {
                    if (SelectedProgress.KanjiProgress < 70)
                        recommendations.Insert(0, $"Focus on Kanji practice - you're at {SelectedProgress.KanjiProgress:F0}%");
                    
                    if (SelectedProgress.VocabularyProgress < 70)
                        recommendations.Insert(0, $"Increase vocabulary study - you're at {SelectedProgress.VocabularyProgress:F0}%");
                    
                    if (SelectedProgress.GrammarProgress < 70)
                        recommendations.Insert(0, $"Practice grammar patterns - you're at {SelectedProgress.GrammarProgress:F0}%");
                }
                
                StudyRecommendations = new ObservableCollection<string>(recommendations.Take(5));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading study recommendations: {ex.Message}");
            }
        }
        
        private async Task LoadExamReadinessAsync()
        {
            try
            {
                if (SelectedProgress != null)
                {
                    var readinessScore = SelectedProgress.OverallProgress;
                    var readinessLevel = readinessScore switch
                    {
                        >= 90 => "Excellent",
                        >= 80 => "Very Good",
                        >= 70 => "Good",
                        >= 60 => "Fair",
                        >= 50 => "Needs Work",
                        _ => "Beginner"
                    };
                    
                    var timeToReady = GetEstimatedTimeToNext(CurrentLevel);
                    
                    var feedback = new List<string>();
                    if (SelectedProgress.KanjiProgress < 80)
                        feedback.Add($"Study {SelectedProgress.RequiredKanji - SelectedProgress.KanjiLearned} more kanji");
                    if (SelectedProgress.VocabularyProgress < 80)
                        feedback.Add($"Learn {SelectedProgress.RequiredVocabulary - SelectedProgress.VocabularyLearned} more vocabulary words");
                    if (SelectedProgress.GrammarProgress < 80)
                        feedback.Add($"Practice {SelectedProgress.RequiredGrammar - SelectedProgress.GrammarLearned} more grammar patterns");
                    
                    ExamReadiness = new ExamReadiness
                    {
                        Level = CurrentLevel,
                        ReadinessScore = readinessScore,
                        ReadinessLevel = readinessLevel,
                        EstimatedTimeToReady = timeToReady,
                        Feedback = feedback,
                        IsReady = readinessScore >= 80
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading exam readiness: {ex.Message}");
            }
        }
        
        private async Task LoadRecentStudySessionsAsync()
        {
            try
            {
                if (CurrentUser != null)
                {
                    var sessions = await _databaseService.GetStudySessionsByUserAsync(CurrentUser.UserId);
                    RecentStudySessions = new ObservableCollection<StudySession>(sessions.Take(5));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading recent study sessions: {ex.Message}");
            }
        }
        
        private async Task LoadCurrentStreakAsync()
        {
            try
            {
                if (CurrentUser != null)
                {
                    var sessions = await _databaseService.GetStudySessionsByUserAsync(CurrentUser.UserId);
                    
                    // Calculate current streak (simplified)
                    var today = DateTime.Today;
                    var streak = 0;
                    
                    for (var date = today; date >= today.AddDays(-30); date = date.AddDays(-1))
                    {
                        var hasSession = sessions.Any(s => s.StartTime.Date == date);
                        if (hasSession)
                        {
                            streak++;
                        }
                        else if (date < today) // Don't break for today if no session yet
                        {
                            break;
                        }
                    }
                    
                    CurrentStreak = streak;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading current streak: {ex.Message}");
            }
        }
        
        public List<string> GetStudyRecommendations(string level)
        {
            return _jlptService.GetRecommendedStudyPlan(level);
        }
        
        public TimeSpan GetEstimatedTimeToNext(string currentLevel)
        {
            var nextLevel = _jlptService.GetNextLevel(currentLevel);
            return _jlptService.GetEstimatedStudyTime(currentLevel, nextLevel);
        }
        
        public JLPTLevelInfo GetLevelInfo(string level)
        {
            return _jlptService.GetLevelInfo(level);
        }
    }
}
