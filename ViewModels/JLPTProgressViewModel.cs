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
        private readonly DatabaseService? _databaseService;
        private readonly JLPTService? _jlptService;
        
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
        
        public JLPTProgressViewModel() : this(null, null)
        {
            // Initialize with sample data when no services are provided
            InitializeWithSampleData();
        }
        
        public JLPTProgressViewModel(
            DatabaseService? databaseService,
            JLPTService? jlptService)
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
            
            // Initialize with sample data if services are not available
            if (_databaseService != null && _jlptService != null)
            {
                _ = InitializeAsync();
            }
            else
            {
                InitializeWithSampleData();
            }
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
            if (_databaseService == null) return;
            
            CurrentUser = await _databaseService.GetUserByUsernameAsync("DefaultUser");
            if (CurrentUser != null)
            {
                CurrentLevel = CurrentUser.CurrentJLPTLevel;
                await LoadProgressAsync();
            }
        }
        
        private void InitializeWithSampleData()
        {
            // Create sample data for demonstration when services are not available
            var sampleProgressList = new List<JLPTProgress>();
            
            var levels = new[] { "N5", "N4", "N3", "N2", "N1" };
            var progressValues = new[] { 75.0, 60.0, 40.0, 25.0, 10.0 };
            
            for (int i = 0; i < levels.Length; i++)
            {
                var levelProgress = CreateSampleJLPTProgress(levels[i], progressValues[i]);
                sampleProgressList.Add(levelProgress);
            }
            
            JLPTProgressList = new ObservableCollection<JLPTProgress>(sampleProgressList);
            SelectedProgress = JLPTProgressList.FirstOrDefault(p => p.Level == CurrentLevel);
            
            // Load sample detailed data
            LoadSampleDetailedData();
            LoadSampleRecommendations();
            LoadSampleExamReadiness();
            
            CurrentStreak = 12; // Sample streak
        }
        
        private JLPTProgress CreateSampleJLPTProgress(string level, double baseProgress)
        {
            var random = new Random();
            var variation = (random.NextDouble() - 0.5) * 20; // ±10% variation
            
            var kanjiProgress = Math.Max(0, Math.Min(100, baseProgress + variation));
            var vocabularyProgress = Math.Max(0, Math.Min(100, baseProgress + variation));
            var grammarProgress = Math.Max(0, Math.Min(100, baseProgress + variation));
            var overallProgress = (kanjiProgress + vocabularyProgress + grammarProgress) / 3;
            
            var levelInfo = GetSampleLevelInfo(level);
            
            return new JLPTProgress
            {
                Level = level,
                KanjiProgress = kanjiProgress,
                VocabularyProgress = vocabularyProgress,
                GrammarProgress = grammarProgress,
                OverallProgress = overallProgress,
                KanjiLearned = (int)(levelInfo.RequiredKanji * kanjiProgress / 100),
                VocabularyLearned = (int)(levelInfo.RequiredVocabulary * vocabularyProgress / 100),
                GrammarLearned = (int)(levelInfo.RequiredGrammar * grammarProgress / 100),
                RequiredKanji = levelInfo.RequiredKanji,
                RequiredVocabulary = levelInfo.RequiredVocabulary,
                RequiredGrammar = levelInfo.RequiredGrammar,
                IsCompleted = overallProgress >= 90
            };
        }
        
        private JLPTLevelInfo GetSampleLevelInfo(string level)
        {
            return level switch
            {
                "N5" => new JLPTLevelInfo { RequiredKanji = 100, RequiredVocabulary = 800, RequiredGrammar = 50 },
                "N4" => new JLPTLevelInfo { RequiredKanji = 300, RequiredVocabulary = 1500, RequiredGrammar = 80 },
                "N3" => new JLPTLevelInfo { RequiredKanji = 650, RequiredVocabulary = 3750, RequiredGrammar = 200 },
                "N2" => new JLPTLevelInfo { RequiredKanji = 1000, RequiredVocabulary = 6000, RequiredGrammar = 400 },
                "N1" => new JLPTLevelInfo { RequiredKanji = 2000, RequiredVocabulary = 10000, RequiredGrammar = 800 },
                _ => new JLPTLevelInfo { RequiredKanji = 100, RequiredVocabulary = 800, RequiredGrammar = 50 }
            };
        }
        
        private async Task LoadProgressAsync()
        {
            if (CurrentUser == null || _databaseService == null || _jlptService == null) 
            {
                InitializeWithSampleData();
                return;
            }
            
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
                InitializeWithSampleData();
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
            if (CurrentUser == null || _databaseService == null) 
            {
                LoadSampleDetailedData();
                return;
            }
            
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
                        Reading = string.Join(", ", kp.Kanji.OnReadings) + ", " + string.Join(", ", kp.Kanji.KunReadings),
                        JLPTLevel = kp.Kanji.JLPTLevel,
                        SRSLevel = kp.SRSLevel,
                        LastReviewed = kp.LastReviewed,
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
                        Reading = vp.Vocabulary.Reading,
                        JLPTLevel = vp.Vocabulary.JLPTLevel,
                        SRSLevel = vp.SRSLevel,
                        LastReviewed = vp.LastReviewed,
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
                        Reading = gp.Grammar.Structure,
                        JLPTLevel = gp.Grammar.JLPTLevel,
                        SRSLevel = gp.UnderstandingLevel,
                        LastReviewed = gp.LastPracticed,
                        IsWeak = gp.UnderstandingLevel <= 2
                    })
                    .ToList();
                
                WeakGrammarItems = new ObservableCollection<StudyItem>(weakGrammar);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading detailed data: {ex.Message}");
                LoadSampleDetailedData();
            }
        }
        
        private void LoadSampleDetailedData()
        {
            // Sample weak kanji items
            var sampleKanji = new List<StudyItem>
            {
                new StudyItem { Id = 1, Content = "日", Meaning = "Day, Sun", Reading = "ニチ, ジツ, ひ", JLPTLevel = "N5", SRSLevel = 1, IsWeak = true },
                new StudyItem { Id = 2, Content = "人", Meaning = "Person", Reading = "ジン, ニン, ひと", JLPTLevel = "N5", SRSLevel = 2, IsWeak = true },
                new StudyItem { Id = 3, Content = "年", Meaning = "Year", Reading = "ネン, とし", JLPTLevel = "N5", SRSLevel = 1, IsWeak = true }
            };
            WeakKanjiItems = new ObservableCollection<StudyItem>(sampleKanji);
            
            // Sample weak vocabulary items
            var sampleVocabulary = new List<StudyItem>
            {
                new StudyItem { Id = 1, Content = "こんにちは", Meaning = "Hello", Reading = "konnichiwa", JLPTLevel = "N5", SRSLevel = 2, IsWeak = true },
                new StudyItem { Id = 2, Content = "ありがとう", Meaning = "Thank you", Reading = "arigatou", JLPTLevel = "N5", SRSLevel = 1, IsWeak = true },
                new StudyItem { Id = 3, Content = "すみません", Meaning = "Excuse me/Sorry", Reading = "sumimasen", JLPTLevel = "N5", SRSLevel = 2, IsWeak = true }
            };
            WeakVocabularyItems = new ObservableCollection<StudyItem>(sampleVocabulary);
            
            // Sample weak grammar items
            var sampleGrammar = new List<StudyItem>
            {
                new StudyItem { Id = 1, Content = "です", Meaning = "Copula (polite)", Reading = "Sentence ending particle", JLPTLevel = "N5", SRSLevel = 1, IsWeak = true },
                new StudyItem { Id = 2, Content = "を", Meaning = "Object marker", Reading = "Particle", JLPTLevel = "N5", SRSLevel = 2, IsWeak = true },
                new StudyItem { Id = 3, Content = "が", Meaning = "Subject marker", Reading = "Particle", JLPTLevel = "N5", SRSLevel = 1, IsWeak = true }
            };
            WeakGrammarItems = new ObservableCollection<StudyItem>(sampleGrammar);
        }
        
        private async Task LoadStudyRecommendationsAsync()
        {
            try
            {
                List<string> recommendations;
                
                if (_jlptService != null)
                {
                    recommendations = GetStudyRecommendations(CurrentLevel);
                }
                else
                {
                    recommendations = GetSampleRecommendations(CurrentLevel);
                }
                
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
                LoadSampleRecommendations();
            }
        }
        
        private void LoadSampleRecommendations()
        {
            var recommendations = GetSampleRecommendations(CurrentLevel);
            StudyRecommendations = new ObservableCollection<string>(recommendations);
        }
        
        private List<string> GetSampleRecommendations(string level)
        {
            return level switch
            {
                "N5" => new List<string>
                {
                    "Master Hiragana and Katakana completely",
                    "Learn basic kanji (100 characters)",
                    "Study essential vocabulary (800 words)",
                    "Practice basic grammar patterns",
                    "Read simple texts and manga"
                },
                "N4" => new List<string>
                {
                    "Expand kanji knowledge (300 characters)",
                    "Build vocabulary (1500 words)",
                    "Study intermediate grammar",
                    "Practice reading short articles",
                    "Listen to slow-paced conversations"
                },
                "N3" => new List<string>
                {
                    "Learn more complex kanji (650 characters)",
                    "Expand vocabulary significantly (3750 words)",
                    "Master intermediate grammar patterns",
                    "Read news articles and short stories",
                    "Listen to normal-speed conversations"
                },
                "N2" => new List<string>
                {
                    "Study advanced kanji (1000 characters)",
                    "Master extensive vocabulary (6000 words)",
                    "Learn complex grammar structures",
                    "Read newspapers and novels",
                    "Listen to news and documentaries"
                },
                "N1" => new List<string>
                {
                    "Master all common kanji (2000+ characters)",
                    "Achieve native-level vocabulary (10000+ words)",
                    "Understand nuanced grammar",
                    "Read academic and technical texts",
                    "Understand rapid natural speech"
                },
                _ => new List<string> { "Start with N5 level studies" }
            };
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
                    
                    TimeSpan timeToReady;
                    if (_jlptService != null)
                    {
                        timeToReady = GetEstimatedTimeToNext(CurrentLevel);
                    }
                    else
                    {
                        // Sample estimated time based on readiness score
                        var weeksToReady = Math.Max(1, (int)((100 - readinessScore) / 10));
                        timeToReady = TimeSpan.FromDays(weeksToReady * 7);
                    }
                    
                    var feedback = new List<string>();
                    if (SelectedProgress.KanjiProgress < 80)
                        feedback.Add($"Study {SelectedProgress.RequiredKanji - SelectedProgress.KanjiLearned} more kanji");
                    if (SelectedProgress.VocabularyProgress < 80)
                        feedback.Add($"Learn {SelectedProgress.RequiredVocabulary - SelectedProgress.VocabularyLearned} more vocabulary words");
                    if (SelectedProgress.GrammarProgress < 80)
                        feedback.Add($"Practice {SelectedProgress.RequiredGrammar - SelectedProgress.GrammarLearned} more grammar patterns");
                    
                    if (feedback.Count == 0)
                        feedback.Add("Great progress! Keep up the consistent study routine.");
                    
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
                LoadSampleExamReadiness();
            }
        }
        
        private void LoadSampleExamReadiness()
        {
            ExamReadiness = new ExamReadiness
            {
                Level = CurrentLevel,
                ReadinessScore = 75,
                ReadinessLevel = "Good",
                EstimatedTimeToReady = TimeSpan.FromDays(28),
                Feedback = new List<string>
                {
                    "Study 25 more kanji characters",
                    "Learn 200 more vocabulary words",
                    "Practice 15 more grammar patterns"
                },
                IsReady = false
            };
        }
        
        private async Task LoadRecentStudySessionsAsync()
        {
            try
            {
                if (CurrentUser != null && _databaseService != null)
                {
                    var sessions = await _databaseService.GetStudySessionsByUserAsync(CurrentUser.UserId);
                    RecentStudySessions = new ObservableCollection<StudySession>(sessions.Take(5));
                }
                else
                {
                    // Sample study sessions
                    var sampleSessions = new List<StudySession>
                    {
                        new StudySession { SessionId = 1, StartTime = DateTime.Now.AddDays(-1), SessionType = "Kanji", ItemsStudied = 25 },
                        new StudySession { SessionId = 2, StartTime = DateTime.Now.AddDays(-2), SessionType = "Vocabulary", ItemsStudied = 30 },
                        new StudySession { SessionId = 3, StartTime = DateTime.Now.AddDays(-3), SessionType = "Grammar", ItemsStudied = 15 }
                    };
                    RecentStudySessions = new ObservableCollection<StudySession>(sampleSessions);
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
                if (CurrentUser != null && _databaseService != null)
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
                else
                {
                    // Sample streak already set in InitializeWithSampleData
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading current streak: {ex.Message}");
            }
        }
        
        public List<string> GetStudyRecommendations(string level)
        {
            if (_jlptService != null)
            {
                return _jlptService.GetRecommendedStudyPlan(level);
            }
            return GetSampleRecommendations(level);
        }
        
        public TimeSpan GetEstimatedTimeToNext(string currentLevel)
        {
            if (_jlptService != null)
            {
                var nextLevel = _jlptService.GetNextLevel(currentLevel);
                return _jlptService.GetEstimatedStudyTime(currentLevel, nextLevel);
            }
            // Sample estimation
            return TimeSpan.FromDays(90);
        }
        
        public JLPTLevelInfo GetLevelInfo(string level)
        {
            if (_jlptService != null)
            {
                return _jlptService.GetLevelInfo(level);
            }
            return GetSampleLevelInfo(level);
        }
    }
}
