using System;
using System.Collections.Generic;
using JapaneseTracker.Services;
using JapaneseTracker.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace JapaneseTracker.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly DatabaseService? _databaseService;
        private readonly ChatGPTJapaneseService? _chatGPTService;
        private readonly JLPTService _jlptService;
        
        private User? _user;
        private string _japaneseQuote = "Loading...";
        private Dictionary<string, int> _studyStatistics = new();
        private int _currentStreak = 0;
        private int _reviewQueueCount = 0;
        
        // Parameterless constructor for XAML design-time support
        public DashboardViewModel()
        {
            _jlptService = new JLPTService();
            
            JLPTLevels = new ObservableCollection<JLPTLevelInfo>(_jlptService.GetAllLevels());
            
            _ = LoadDashboardDataAsync();
        }
        
        public DashboardViewModel(
            DatabaseService databaseService,
            ChatGPTJapaneseService chatGPTService,
            JLPTService jlptService)
        {
            _databaseService = databaseService;
            _chatGPTService = chatGPTService;
            _jlptService = jlptService;
            
            JLPTLevels = new ObservableCollection<JLPTLevelInfo>(_jlptService.GetAllLevels());
            
            _ = LoadDashboardDataAsync();
        }
        
        public User? User
        {
            get => _user;
            set => SetProperty(ref _user, value);
        }
        
        public string JapaneseQuote
        {
            get => _japaneseQuote;
            set => SetProperty(ref _japaneseQuote, value);
        }
        
        public Dictionary<string, int> StudyStatistics
        {
            get => _studyStatistics;
            set => SetProperty(ref _studyStatistics, value);
        }
        
        public int CurrentStreak
        {
            get => _currentStreak;
            set => SetProperty(ref _currentStreak, value);
        }
        
        public int ReviewQueueCount
        {
            get => _reviewQueueCount;
            set => SetProperty(ref _reviewQueueCount, value);
        }
        
        public ObservableCollection<JLPTLevelInfo> JLPTLevels { get; }
        
        private async Task LoadDashboardDataAsync()
        {
            try
            {
                // If we have database service, use it; otherwise use mock data
                if (_databaseService != null)
                {
                    // Get current user (for demo, use the default user)
                    var user = await _databaseService.GetUserByUsernameAsync("DefaultUser");
                    if (user != null)
                    {
                        User = user;
                        CurrentStreak = user.StudyStreak;
                        
                        // Load study statistics
                        StudyStatistics = await _databaseService.GetStudyStatisticsAsync(user.UserId);
                        
                        // Get review queue count
                        var kanjiReviews = await _databaseService.GetKanjiReviewQueueAsync(user.UserId);
                        var vocabReviews = await _databaseService.GetVocabularyReviewQueueAsync(user.UserId);
                        ReviewQueueCount = kanjiReviews.Count + vocabReviews.Count;
                    }
                    
                    // Load Japanese tip of the day
                    if (_chatGPTService != null)
                    {
                        JapaneseQuote = await _chatGPTService.GetJapaneseTipOfTheDayAsync();
                    }
                }
                else
                {
                    // Mock data for demo purposes
                    User = new User
                    {
                        UserId = 1,
                        Username = "Demo User",
                        CurrentJLPTLevel = "N4",
                        StudyStreak = 15,
                        CreatedAt = DateTime.UtcNow.AddDays(-45),
                        LastActive = DateTime.UtcNow
                    };
                    
                    CurrentStreak = User.StudyStreak;
                    
                    // Mock study statistics
                    StudyStatistics = new Dictionary<string, int>
                    {
                        ["LearnedKanji"] = 247,
                        ["LearnedVocabulary"] = 892,
                        ["LearnedGrammar"] = 45,
                        ["StudyHours"] = 67,
                        ["ReviewsSRS"] = 1234
                    };
                    
                    ReviewQueueCount = 23;
                    
                    // Mock Japanese tip
                    var tips = new[]
                    {
                        "頑張って！(Ganbatte!) - Good luck! This is a common way to encourage someone in Japanese.",
                        "The particle は (wa) is written as は but pronounced 'wa' when used as a topic marker.",
                        "Practice makes perfect! Try to study Japanese for at least 15 minutes every day.",
                        "こんにちは (Konnichiwa) can be used from late morning until late afternoon.",
                        "Kanji radicals are the building blocks of kanji characters. Learning them will help you understand new kanji more easily."
                    };
                    
                    var random = new Random();
                    JapaneseQuote = tips[random.Next(tips.Length)];
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading dashboard: {ex.Message}");
                JapaneseQuote = "Welcome to your Japanese learning journey! 頑張って！";
                
                // Fallback mock data
                User = new User { Username = "Guest User", StudyStreak = 0 };
                StudyStatistics = new Dictionary<string, int>();
                ReviewQueueCount = 0;
            }
        }
        
        public void RefreshData()
        {
            _ = LoadDashboardDataAsync();
        }
    }
}
