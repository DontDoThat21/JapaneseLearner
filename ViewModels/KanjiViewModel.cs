using System;
using System.Collections.Generic;
using JapaneseTracker.Services;
using JapaneseTracker.Models;
using JapaneseTracker.Commands;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JapaneseTracker.ViewModels
{
    public class KanjiViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly KanjiRadicalService _kanjiRadicalService;
        private readonly SRSCalculationService _srsService;
        private readonly ChatGPTJapaneseService _chatGPTService;
        
        private ObservableCollection<KanjiDisplayModel> _kanjiList = new();
        private KanjiDisplayModel? _selectedKanji;
        private string _selectedJLPTLevel = "N5";
        private string _searchText = "";
        private bool _isLoading = false;
        private User? _currentUser;
        
        // Parameterless constructor for XAML design-time support
        public KanjiViewModel()
        {
            // Initialize with default services for design-time
            _databaseService = new DatabaseService(null!);
            _kanjiRadicalService = new KanjiRadicalService();
            _srsService = new SRSCalculationService();
            _chatGPTService = new ChatGPTJapaneseService(new MockChatGPTJapaneseService());
            
            // Commands
            LoadKanjiCommand = new RelayCommand(async () => await LoadKanjiAsync());
            SelectKanjiCommand = new RelayCommand<KanjiDisplayModel>(SelectKanji);
            StudyKanjiCommand = new RelayCommand<KanjiDisplayModel>(async (kanji) => await StudyKanjiAsync(kanji));
            GetMnemonicCommand = new RelayCommand<KanjiDisplayModel>(async (kanji) => await GetMnemonicAsync(kanji));
            SearchKanjiCommand = new RelayCommand(async () => await SearchKanjiAsync());
            
            JLPTLevels = new ObservableCollection<string> { "N5", "N4", "N3", "N2", "N1" };
        }

        public KanjiViewModel(
            DatabaseService databaseService,
            KanjiRadicalService kanjiRadicalService,
            SRSCalculationService srsService,
            ChatGPTJapaneseService chatGPTService)
        {
            _databaseService = databaseService;
            _kanjiRadicalService = kanjiRadicalService;
            _srsService = srsService;
            _chatGPTService = chatGPTService;
            
            // Commands
            LoadKanjiCommand = new RelayCommand(async () => await LoadKanjiAsync());
            SelectKanjiCommand = new RelayCommand<KanjiDisplayModel>(SelectKanji);
            StudyKanjiCommand = new RelayCommand<KanjiDisplayModel>(async (kanji) => await StudyKanjiAsync(kanji));
            GetMnemonicCommand = new RelayCommand<KanjiDisplayModel>(async (kanji) => await GetMnemonicAsync(kanji));
            SearchKanjiCommand = new RelayCommand(async () => await SearchKanjiAsync());
            
            JLPTLevels = new ObservableCollection<string> { "N5", "N4", "N3", "N2", "N1" };
            
            // Initialize
            _ = InitializeAsync();
        }
        
        public ObservableCollection<KanjiDisplayModel> KanjiList
        {
            get => _kanjiList;
            set 
            { 
                if (SetProperty(ref _kanjiList, value))
                {
                    OnPropertyChanged(nameof(TotalKanjiCount));
                    OnPropertyChanged(nameof(LearnedCount));
                    OnPropertyChanged(nameof(ReviewDueCount));
                }
            }
        }
        
        public KanjiDisplayModel? SelectedKanji
        {
            get => _selectedKanji;
            set => SetProperty(ref _selectedKanji, value);
        }
        
        public string SelectedJLPTLevel
        {
            get => _selectedJLPTLevel;
            set 
            { 
                if (SetProperty(ref _selectedJLPTLevel, value))
                {
                    _ = LoadKanjiAsync();
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
        
        public ICommand LoadKanjiCommand { get; }
        public ICommand SelectKanjiCommand { get; }
        public ICommand StudyKanjiCommand { get; }
        public ICommand GetMnemonicCommand { get; }
        public ICommand SearchKanjiCommand { get; }
        
        // Computed Properties
        public int LearnedCount => KanjiList.Count(k => k.SRSLevel > 2);
        public int ReviewDueCount => KanjiList.Count(k => k.IsReviewDue);
        public int TotalKanjiCount => KanjiList.Count;
        
        private async Task InitializeAsync()
        {
            CurrentUser = await _databaseService.GetUserByUsernameAsync("DefaultUser");
            if (CurrentUser != null)
            {
                await LoadKanjiAsync();
            }
        }
        
        private async Task LoadKanjiAsync()
        {
            IsLoading = true;
            try
            {
                var kanjiList = await _databaseService.GetKanjiByJLPTLevelAsync(SelectedJLPTLevel);
                var displayModels = new List<KanjiDisplayModel>();
                
                foreach (var kanji in kanjiList)
                {
                    var progress = CurrentUser != null 
                        ? await _databaseService.GetKanjiProgressAsync(CurrentUser.UserId, kanji.KanjiId)
                        : null;
                    displayModels.Add(new KanjiDisplayModel(kanji, progress));
                }
                
                KanjiList = new ObservableCollection<KanjiDisplayModel>(displayModels);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading kanji: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private void SelectKanji(KanjiDisplayModel? kanji)
        {
            SelectedKanji = kanji;
        }
        
        private async Task StudyKanjiAsync(KanjiDisplayModel? kanjiDisplayModel)
        {
            if (kanjiDisplayModel == null || CurrentUser == null) return;
            
            var kanji = kanjiDisplayModel.Kanji;
            
            try
            {
                // Get or create progress record
                var progress = await _databaseService.GetKanjiProgressAsync(CurrentUser.UserId, kanji.KanjiId);
                if (progress == null)
                {
                    progress = new KanjiProgress
                    {
                        UserId = CurrentUser.UserId,
                        KanjiId = kanji.KanjiId,
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
                
                await _databaseService.UpdateKanjiProgressAsync(progress);
                
                // Update the display model
                kanjiDisplayModel.Progress = progress;
                
                // Refresh the list to update computed properties
                OnPropertyChanged(nameof(LearnedCount));
                OnPropertyChanged(nameof(ReviewDueCount));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error studying kanji: {ex.Message}");
            }
        }
        
        private async Task GetMnemonicAsync(KanjiDisplayModel? kanjiDisplayModel)
        {
            if (kanjiDisplayModel == null) return;
            
            var kanji = kanjiDisplayModel.Kanji;
            
            try
            {
                var mnemonic = await _chatGPTService.GenerateMnemonicAsync(kanji.Character, kanji.Meaning);
                // In a real app, you would display this in a dialog or dedicated view
                System.Diagnostics.Debug.WriteLine($"Mnemonic for {kanji.Character}: {mnemonic}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting mnemonic: {ex.Message}");
            }
        }
        
        private async Task SearchKanjiAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText)) return;
            
            IsLoading = true;
            try
            {
                var allKanji = await _databaseService.GetKanjiByJLPTLevelAsync(SelectedJLPTLevel);
                var filteredKanji = allKanji.Where(k => 
                    k.Character.Contains(SearchText) ||
                    k.Meaning.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    k.OnReadings.Any(r => r.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                    k.KunReadings.Any(r => r.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                ).ToList();
                
                var displayModels = new List<KanjiDisplayModel>();
                
                foreach (var kanji in filteredKanji)
                {
                    var progress = CurrentUser != null 
                        ? await _databaseService.GetKanjiProgressAsync(CurrentUser.UserId, kanji.KanjiId)
                        : null;
                    displayModels.Add(new KanjiDisplayModel(kanji, progress));
                }
                
                KanjiList = new ObservableCollection<KanjiDisplayModel>(displayModels);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error searching kanji: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        public KanjiProgress? GetKanjiProgress(int kanjiId)
        {
            return KanjiList.FirstOrDefault(k => k.KanjiId == kanjiId)?.Progress;
        }
        
        public string GetKanjiMasteryLevel(int kanjiId)
        {
            var progress = GetKanjiProgress(kanjiId);
            if (progress == null) return "New";
            
            return _srsService.GetMasteryLevel(progress.SRSLevel, progress.AccuracyRate);
        }
        
        public int GetKanjiSRSLevel(int kanjiId)
        {
            var progress = GetKanjiProgress(kanjiId);
            return progress?.SRSLevel ?? 0;
        }
    }
}