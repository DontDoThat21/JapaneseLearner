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
        private readonly DatabaseService _databaseService;
        private readonly ChatGPTJapaneseService _chatGPTService;
        private readonly JLPTService _jlptService;
        
        private User? _user;
        private string _japaneseQuote = "Loading...";
        private Dictionary<string, int> _studyStatistics = new();
        private int _currentStreak = 0;
        private int _reviewQueueCount = 0;
        
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
                JapaneseQuote = await _chatGPTService.GetJapaneseTipOfTheDayAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading dashboard: {ex.Message}");
                JapaneseQuote = "Welcome to your Japanese learning journey! 頑張って！";
            }
        }
        
        public void RefreshData()
        {
            _ = LoadDashboardDataAsync();
        }
    }
}
