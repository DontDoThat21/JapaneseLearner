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
        
        private ObservableCollection<Vocabulary> _vocabularyList = new();
        private ObservableCollection<VocabularyProgress> _vocabularyProgress = new();
        private Vocabulary? _selectedVocabulary;
        private string _selectedJLPTLevel = "N5";
        private string _searchText = "";
        private bool _isLoading = false;
        private User? _currentUser;
        
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
            SelectVocabularyCommand = new RelayCommand<Vocabulary>(async (vocab) => await SelectVocabularyAsync(vocab));
            StudyVocabularyCommand = new RelayCommand<Vocabulary>(async (vocab) => await StudyVocabularyAsync(vocab));
            SearchVocabularyCommand = new RelayCommand(async () => await SearchVocabularyAsync());
            
            JLPTLevels = new ObservableCollection<string> { "N5", "N4", "N3", "N2", "N1" };
            
            // Initialize
            _ = InitializeAsync();
        }
        
        public ObservableCollection<Vocabulary> VocabularyList
        {
            get => _vocabularyList;
            set => SetProperty(ref _vocabularyList, value);
        }
        
        public ObservableCollection<VocabularyProgress> VocabularyProgress
        {
            get => _vocabularyProgress;
            set => SetProperty(ref _vocabularyProgress, value);
        }
        
        public Vocabulary? SelectedVocabulary
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
                VocabularyList = new ObservableCollection<Vocabulary>(vocabularyList);
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
        
        private async Task SelectVocabularyAsync(Vocabulary? vocabulary)
        {
            // TODO Not sure what this method was intended to do
            SelectedVocabulary = vocabulary;
            // Perform any additional async operations if needed in the future
            await Task.CompletedTask;
        }
        
        private async Task StudyVocabularyAsync(Vocabulary? vocabulary)
        {
            if (vocabulary == null || CurrentUser == null) return;
            
            try
            {
                // Get or create progress record
                var progress = await _databaseService.GetVocabularyProgressAsync(CurrentUser.UserId, vocabulary.VocabId);
                if (progress == null)
                {
                    progress = new VocabularyProgress
                    {
                        UserId = CurrentUser.UserId,
                        VocabId = vocabulary.VocabId,
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
                await LoadVocabularyProgressAsync();
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
                
                VocabularyList = new ObservableCollection<Vocabulary>(filteredVocabulary);
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
    }
}
