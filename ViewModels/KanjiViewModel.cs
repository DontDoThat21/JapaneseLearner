using JapaneseTracker.Services;
using JapaneseTracker.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace JapaneseTracker.ViewModels
{
    public class KanjiViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly KanjiRadicalService _kanjiRadicalService;
        private readonly SRSCalculationService _srsService;
        private readonly ChatGPTJapaneseService _chatGPTService;
        
        private ObservableCollection<Kanji> _kanjiList = new();
        private ObservableCollection<KanjiProgress> _kanjiProgress = new();
        private Kanji? _selectedKanji;
        private string _selectedJLPTLevel = "N5";
        private string _searchText = "";
        private bool _isLoading = false;
        private User? _currentUser;
        
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
            SelectKanjiCommand = new RelayCommand<Kanji>(SelectKanji);
            StudyKanjiCommand = new RelayCommand<Kanji>(async (kanji) => await StudyKanjiAsync(kanji));
            GetMnemonicCommand = new RelayCommand<Kanji>(async (kanji) => await GetMnemonicAsync(kanji));
            SearchKanjiCommand = new RelayCommand(async () => await SearchKanjiAsync());
            
            JLPTLevels = new ObservableCollection<string> { "N5", "N4", "N3", "N2", "N1" };
            
            // Initialize
            _ = InitializeAsync();
        }
        
        public ObservableCollection<Kanji> KanjiList
        {
            get => _kanjiList;
            set => SetProperty(ref _kanjiList, value);
        }
        
        public ObservableCollection<KanjiProgress> KanjiProgress
        {
            get => _kanjiProgress;
            set => SetProperty(ref _kanjiProgress, value);
        }
        
        public Kanji? SelectedKanji
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
        
        private async Task InitializeAsync()
        {
            CurrentUser = await _databaseService.GetUserByUsernameAsync("DefaultUser");
            if (CurrentUser != null)
            {
                await LoadKanjiAsync();
                await LoadKanjiProgressAsync();
            }
        }
        
        private async Task LoadKanjiAsync()
        {
            IsLoading = true;
            try
            {
                var kanjiList = await _databaseService.GetKanjiByJLPTLevelAsync(SelectedJLPTLevel);
                KanjiList = new ObservableCollection<Kanji>(kanjiList);
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
        
        private async Task LoadKanjiProgressAsync()
        {
            if (CurrentUser == null) return;
            
            try
            {
                var progressList = new List<KanjiProgress>();
                foreach (var kanji in KanjiList)
                {
                    var progress = await _databaseService.GetKanjiProgressAsync(CurrentUser.UserId, kanji.KanjiId);
                    if (progress != null)
                    {
                        progressList.Add(progress);
                    }
                }
                KanjiProgress = new ObservableCollection<KanjiProgress>(progressList);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading kanji progress: {ex.Message}");
            }
        }
        
        private void SelectKanji(Kanji? kanji)
        {
            SelectedKanji = kanji;
        }
        
        private async Task StudyKanjiAsync(Kanji? kanji)
        {
            if (kanji == null || CurrentUser == null) return;
            
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
                await LoadKanjiProgressAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error studying kanji: {ex.Message}");
            }
        }
        
        private async Task GetMnemonicAsync(Kanji? kanji)
        {
            if (kanji == null) return;
            
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
                
                KanjiList = new ObservableCollection<Kanji>(filteredKanji);
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
            return KanjiProgress.FirstOrDefault(kp => kp.KanjiId == kanjiId);
        }
        
        public string GetKanjiMasteryLevel(int kanjiId)
        {
            var progress = GetKanjiProgress(kanjiId);
            if (progress == null) return "New";
            
            return _srsService.GetMasteryLevel(progress.SRSLevel, progress.AccuracyRate);
        }
    }
}
