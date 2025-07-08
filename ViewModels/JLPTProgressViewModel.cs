using JapaneseTracker.Services;
using JapaneseTracker.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

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
        
        public JLPTProgressViewModel(
            DatabaseService databaseService,
            JLPTService jlptService)
        {
            _databaseService = databaseService;
            _jlptService = jlptService;
            
            // Commands
            LoadProgressCommand = new RelayCommand(async () => await LoadProgressAsync());
            SelectLevelCommand = new RelayCommand<string>(SelectLevel);
            
            JLPTLevels = new ObservableCollection<string> { "N5", "N4", "N3", "N2", "N1" };
            
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
        
        public ICommand LoadProgressCommand { get; }
        public ICommand SelectLevelCommand { get; }
        
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
