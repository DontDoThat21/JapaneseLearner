using JapaneseTracker.Services;
using JapaneseTracker.Models;
using JapaneseTracker.Commands;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JapaneseTracker.ViewModels
{
    public class VocabularyViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly SRSCalculationService _srsService;
        private readonly ChatGPTJapaneseService _chatGPTService;
        
        private ObservableCollection<VocabularyDisplayModel> _vocabularyList = new();
        private ObservableCollection<VocabularyProgress> _vocabularyProgress = new();
        private VocabularyDisplayModel? _selectedVocabulary;
        private string _selectedJLPTLevel = "N5";
        private string _searchText = "";
        private bool _isLoading = false;
        private User? _currentUser;
        
        // Parameterless constructor for XAML design-time support
        public VocabularyViewModel()
        {
            _databaseService = null!;
            _srsService = null!;
            _chatGPTService = null!;
            
            // Initialize collections for design-time
            JLPTLevels = new ObservableCollection<string> { "N5", "N4", "N3", "N2", "N1" };
            
            // Initialize commands with no-op implementations for design-time
            LoadVocabularyCommand = new RelayCommand(() => { });
            SelectVocabularyCommand = new RelayCommand<VocabularyDisplayModel>((vocab) => { SelectedVocabulary = vocab; });
            StudyVocabularyCommand = new RelayCommand<VocabularyDisplayModel>((vocab) => { });
            SearchVocabularyCommand = new RelayCommand(() => { });
            
            // Mock data for design-time preview
            VocabularyList = new ObservableCollection<VocabularyDisplayModel>
            {
                new VocabularyDisplayModel 
                { 
                    Vocabulary = new Vocabulary { VocabId = 1, Word = "こんにちは", Reading = "こんにちは", Meaning = "Hello", JLPTLevel = "N5", PartOfSpeech = "Greeting" },
                    Progress = new VocabularyProgress { SRSLevel = 2, CorrectCount = 5, IncorrectCount = 1 }
                },
                new VocabularyDisplayModel 
                { 
                    Vocabulary = new Vocabulary { VocabId = 2, Word = "ありがとう", Reading = "ありがとう", Meaning = "Thank you", JLPTLevel = "N5", PartOfSpeech = "Expression" },
                    Progress = new VocabularyProgress { SRSLevel = 4, CorrectCount = 12, IncorrectCount = 2 }
                },
                new VocabularyDisplayModel 
                { 
                    Vocabulary = new Vocabulary { VocabId = 3, Word = "学校", Reading = "がっこう", Meaning = "School", JLPTLevel = "N5", PartOfSpeech = "Noun" },
                    Progress = null // New word, not studied yet
                }
            };
            
            // Mock user for design-time
            CurrentUser = new User { UserId = 1, Username = "TestUser", StudyStreak = 15 };
        }
        
        // Dependency injection constructor
        public VocabularyViewModel(
            DatabaseService databaseService,
            SRSCalculationService srsService,
            ChatGPTJapaneseService chatGPTService)
        {
            _databaseService = databaseService;
            _srsService = srsService;
            _chatGPTService = chatGPTService;
            
            // Commands
            LoadVocabularyCommand = new RelayCommand(async () => await LoadVocabularyAsync());
            SelectVocabularyCommand = new RelayCommand<VocabularyDisplayModel>(async (vocab) => await SelectVocabularyAsync(vocab));
            StudyVocabularyCommand = new RelayCommand<VocabularyDisplayModel>(async (vocab) => await StudyVocabularyAsync(vocab));
            SearchVocabularyCommand = new RelayCommand(async () => await SearchVocabularyAsync());
            
            JLPTLevels = new ObservableCollection<string> { "N5", "N4", "N3", "N2", "N1" };
            
            // Initialize
            _ = InitializeAsync();
        }
        
        public ObservableCollection<VocabularyDisplayModel> VocabularyList
        {
            get => _vocabularyList;
            set => SetProperty(ref _vocabularyList, value);
        }
        
        public ObservableCollection<VocabularyProgress> VocabularyProgress
        {
            get => _vocabularyProgress;
            set => SetProperty(ref _vocabularyProgress, value);
        }
        
        public VocabularyDisplayModel? SelectedVocabulary
        {
            get => _selectedVocabulary;
            set => SetProperty(ref _selectedVocabulary, value);
        }
        
        public string SelectedJLPTLevel
        {
            get => _selectedJLPTLevel;
            set 
            { 
                if (SetProperty(ref _selectedJLPTLevel, value))
                {
                    _ = LoadVocabularyAsync();
                }
            }
        }
        
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
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
        
        public ICommand LoadVocabularyCommand { get; }
        public ICommand SelectVocabularyCommand { get; }
        public ICommand StudyVocabularyCommand { get; }
        public ICommand SearchVocabularyCommand { get; }
        
        private async Task InitializeAsync()
        {
            CurrentUser = await _databaseService.GetUserByUsernameAsync("DefaultUser");
            if (CurrentUser != null)
            {
                await LoadVocabularyAsync();
                await LoadVocabularyProgressAsync();
            }
        }
        
        private async Task LoadVocabularyAsync()
        {
            IsLoading = true;
            try
            {
                var vocabularyList = await _databaseService.GetVocabularyByJLPTLevelAsync(SelectedJLPTLevel);
                
                // Create display models combining vocabulary and progress data
                var displayModels = new List<VocabularyDisplayModel>();
                foreach (var vocabulary in vocabularyList)
                {
                    var progress = CurrentUser != null 
                        ? await _databaseService.GetVocabularyProgressAsync(CurrentUser.UserId, vocabulary.VocabId)
                        : null;
                    
                    displayModels.Add(new VocabularyDisplayModel
                    {
                        Vocabulary = vocabulary,
                        Progress = progress
                    });
                }
                
                VocabularyList = new ObservableCollection<VocabularyDisplayModel>(displayModels);
                NotifyStatsChanged();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading vocabulary: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private async Task LoadVocabularyProgressAsync()
        {
            if (CurrentUser == null) return;
            
            try
            {
                var progressList = new List<VocabularyProgress>();
                foreach (var vocabulary in VocabularyList)
                {
                    var progress = await _databaseService.GetVocabularyProgressAsync(CurrentUser.UserId, vocabulary.VocabId);
                    if (progress != null)
                    {
                        progressList.Add(progress);
                    }
                }
                VocabularyProgress = new ObservableCollection<VocabularyProgress>(progressList);
                NotifyStatsChanged();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading vocabulary progress: {ex.Message}");
            }
        }
        
        private void SelectVocabulary(Vocabulary? vocabulary)
        {
            SelectedVocabulary = vocabulary;
        }
        
        private async Task SelectVocabularyAsync(VocabularyDisplayModel? vocabulary)
        {
            // TODO Not sure what this method was intended to do
            SelectedVocabulary = vocabulary;
            // Perform any additional async operations if needed in the future
            await Task.CompletedTask;
        }
        
        private async Task StudyVocabularyAsync(VocabularyDisplayModel? vocabularyModel)
        {
            if (vocabularyModel?.Vocabulary == null || CurrentUser == null) return;
            
            try
            {
                // Get or create progress record
                var progress = await _databaseService.GetVocabularyProgressAsync(CurrentUser.UserId, vocabularyModel.VocabId);
                if (progress == null)
                {
                    progress = new VocabularyProgress
                    {
                        UserId = CurrentUser.UserId,
                        VocabId = vocabularyModel.VocabId,
                        SRSLevel = 0,
                        NextReviewDate = DateTime.UtcNow
                    };
                }
                
                // For now, simulate a correct answer (in a real app, this would be based on user input)
                var wasCorrect = true; // This would come from a quiz/practice session
                
                // Update SRS
                progress.NextReviewDate = _srsService.CalculateNextReview(progress.SRSLevel, wasCorrect);
                progress.SRSLevel = _srsService.CalculateNewSRSLevel(progress.SRSLevel, wasCorrect);
                progress.LastReviewed = DateTime.UtcNow;
                
                if (wasCorrect)
                    progress.CorrectCount++;
                else
                    progress.IncorrectCount++;
                
                await _databaseService.UpdateVocabularyProgressAsync(progress);
                await LoadVocabularyAsync(); // Refresh the display models
                NotifyStatsChanged();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error studying vocabulary: {ex.Message}");
            }
        }
        
        private async Task SearchVocabularyAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText)) return;
            
            IsLoading = true;
            try
            {
                var allVocabulary = await _databaseService.GetVocabularyByJLPTLevelAsync(SelectedJLPTLevel);
                var filteredVocabulary = allVocabulary.Where(v => 
                    v.Word.Contains(SearchText) ||
                    v.Reading.Contains(SearchText) ||
                    v.Meaning.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    v.PartOfSpeech.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                ).ToList();
                
                // Create display models for filtered results
                var displayModels = new List<VocabularyDisplayModel>();
                foreach (var vocabulary in filteredVocabulary)
                {
                    var progress = CurrentUser != null 
                        ? await _databaseService.GetVocabularyProgressAsync(CurrentUser.UserId, vocabulary.VocabId)
                        : null;
                    
                    displayModels.Add(new VocabularyDisplayModel
                    {
                        Vocabulary = vocabulary,
                        Progress = progress
                    });
                }
                
                VocabularyList = new ObservableCollection<VocabularyDisplayModel>(displayModels);
                NotifyStatsChanged();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error searching vocabulary: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        public VocabularyProgress? GetVocabularyProgress(int vocabId)
        {
            return VocabularyProgress.FirstOrDefault(vp => vp.VocabId == vocabId);
        }
        
        public string GetVocabularyMasteryLevel(int vocabId)
        {
            var progress = GetVocabularyProgress(vocabId);
            if (progress == null) return "New";
            
            return _srsService.GetMasteryLevel(progress.SRSLevel, progress.AccuracyRate);
        }
        
        // Computed properties for statistics cards
        public int LearnedCount
        {
            get
            {
                if (CurrentUser == null) return 0;
                
                // Count vocabulary with SRS level > 2 (considered "learned")
                return VocabularyList.Count(v => v.SRSLevel > 2);
            }
        }
        
        public int ReviewDueCount
        {
            get
            {
                if (CurrentUser == null) return 0;
                
                // Count vocabulary that needs review today
                return VocabularyList.Count(v => v.IsReviewDue);
            }
        }
        
        private void NotifyStatsChanged()
        {
            OnPropertyChanged(nameof(LearnedCount));
            OnPropertyChanged(nameof(ReviewDueCount));
        }
    }
}
