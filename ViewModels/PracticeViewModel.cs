using JapaneseTracker.Services;
using JapaneseTracker.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace JapaneseTracker.ViewModels
{
    public class PracticeViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly SRSCalculationService _srsService;
        
        private ObservableCollection<KanaCharacter> _kanaCharacters = new();
        private ObservableCollection<KanaProgress> _kanaProgress = new();
        private KanaCharacter? _currentKana;
        private string _selectedKanaType = "Hiragana";
        private string _userInput = "";
        private bool _showAnswer = false;
        private bool _isLoading = false;
        private User? _currentUser;
        private int _currentIndex = 0;
        private int _correctAnswers = 0;
        private int _totalAnswers = 0;
        
        public PracticeViewModel(
            DatabaseService databaseService,
            SRSCalculationService srsService)
        {
            _databaseService = databaseService;
            _srsService = srsService;
            
            // Commands
            LoadKanaCommand = new RelayCommand(async () => await LoadKanaAsync());
            NextKanaCommand = new RelayCommand(NextKana);
            CheckAnswerCommand = new RelayCommand(CheckAnswer);
            SubmitAnswerCommand = new RelayCommand(async () => await SubmitAnswerAsync());
            ShowAnswerCommand = new RelayCommand(() => ShowAnswer = true);
            StartPracticeCommand = new RelayCommand(async () => await StartPracticeAsync());
            
            KanaTypes = new ObservableCollection<string> { "Hiragana", "Katakana" };
            
            // Initialize
            _ = InitializeAsync();
        }
        
        public ObservableCollection<KanaCharacter> KanaCharacters
        {
            get => _kanaCharacters;
            set => SetProperty(ref _kanaCharacters, value);
        }
        
        public ObservableCollection<KanaProgress> KanaProgress
        {
            get => _kanaProgress;
            set => SetProperty(ref _kanaProgress, value);
        }
        
        public KanaCharacter? CurrentKana
        {
            get => _currentKana;
            set => SetProperty(ref _currentKana, value);
        }
        
        public string SelectedKanaType
        {
            get => _selectedKanaType;
            set 
            { 
                if (SetProperty(ref _selectedKanaType, value))
                {
                    _ = LoadKanaAsync();
                }
            }
        }
        
        public string UserInput
        {
            get => _userInput;
            set => SetProperty(ref _userInput, value);
        }
        
        public bool ShowAnswer
        {
            get => _showAnswer;
            set => SetProperty(ref _showAnswer, value);
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
        
        public int CurrentIndex
        {
            get => _currentIndex;
            set => SetProperty(ref _currentIndex, value);
        }
        
        public int CorrectAnswers
        {
            get => _correctAnswers;
            set => SetProperty(ref _correctAnswers, value);
        }
        
        public int TotalAnswers
        {
            get => _totalAnswers;
            set => SetProperty(ref _totalAnswers, value);
        }
        
        public double AccuracyRate => TotalAnswers > 0 ? (double)CorrectAnswers / TotalAnswers * 100 : 0;
        
        public ObservableCollection<string> KanaTypes { get; }
        
        public ICommand LoadKanaCommand { get; }
        public ICommand NextKanaCommand { get; }
        public ICommand CheckAnswerCommand { get; }
        public ICommand SubmitAnswerCommand { get; }
        public ICommand ShowAnswerCommand { get; }
        public ICommand StartPracticeCommand { get; }
        
        private async Task InitializeAsync()
        {
            CurrentUser = await _databaseService.GetUserByUsernameAsync("DefaultUser");
            if (CurrentUser != null)
            {
                await LoadKanaAsync();
                await LoadKanaProgressAsync();
            }
        }
        
        private async Task LoadKanaAsync()
        {
            IsLoading = true;
            try
            {
                var kanaList = await _databaseService.GetKanaCharactersByTypeAsync(SelectedKanaType);
                KanaCharacters = new ObservableCollection<KanaCharacter>(kanaList);
                
                if (KanaCharacters.Any())
                {
                    CurrentIndex = 0;
                    CurrentKana = KanaCharacters[CurrentIndex];
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading kana: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private async Task LoadKanaProgressAsync()
        {
            if (CurrentUser == null) return;
            
            try
            {
                var progressList = await _databaseService.GetKanaProgressAsync(CurrentUser.UserId, SelectedKanaType);
                KanaProgress = new ObservableCollection<KanaProgress>(progressList);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading kana progress: {ex.Message}");
            }
        }
        
        private void NextKana()
        {
            if (KanaCharacters.Any())
            {
                CurrentIndex = (CurrentIndex + 1) % KanaCharacters.Count;
                CurrentKana = KanaCharacters[CurrentIndex];
                UserInput = "";
                ShowAnswer = false;
            }
        }
        
        private void CheckAnswer()
        {
            if (CurrentKana == null || string.IsNullOrWhiteSpace(UserInput)) return;
            
            var isCorrect = string.Equals(UserInput.Trim(), CurrentKana.Romaji, StringComparison.OrdinalIgnoreCase);
            
            if (isCorrect)
            {
                CorrectAnswers++;
            }
            
            TotalAnswers++;
            ShowAnswer = true;
            
            OnPropertyChanged(nameof(AccuracyRate));
        }
        
        private async Task SubmitAnswerAsync()
        {
            if (CurrentKana == null || CurrentUser == null) return;
            
            try
            {
                var isCorrect = string.Equals(UserInput.Trim(), CurrentKana.Romaji, StringComparison.OrdinalIgnoreCase);
                
                // Get or create progress record
                var progress = KanaProgress.FirstOrDefault(kp => kp.KanaId == CurrentKana.KanaId);
                if (progress == null)
                {
                    progress = new KanaProgress
                    {
                        UserId = CurrentUser.UserId,
                        KanaId = CurrentKana.KanaId,
                        Mastered = false,
                        PracticeCount = 0
                    };
                    KanaProgress.Add(progress);
                }
                
                // Update progress
                progress.PracticeCount++;
                progress.LastPracticed = DateTime.UtcNow;
                
                if (isCorrect)
                {
                    progress.CorrectCount++;
                    // Mark as mastered if accuracy is high
                    if (progress.AccuracyRate >= 90 && progress.TotalAttempts >= 5)
                    {
                        progress.Mastered = true;
                    }
                }
                else
                {
                    progress.IncorrectCount++;
                }
                
                await _databaseService.UpdateKanaProgressAsync(progress);
                
                // Move to next character
                NextKana();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error submitting answer: {ex.Message}");
            }
        }
        
        private async Task StartPracticeAsync()
        {
            CorrectAnswers = 0;
            TotalAnswers = 0;
            
            if (KanaCharacters.Any())
            {
                // Shuffle the characters for variety
                var random = new Random();
                var shuffled = KanaCharacters.OrderBy(x => random.Next()).ToList();
                KanaCharacters = new ObservableCollection<KanaCharacter>(shuffled);
                
                CurrentIndex = 0;
                CurrentKana = KanaCharacters[CurrentIndex];
                UserInput = "";
                ShowAnswer = false;
            }
            
            await LoadKanaProgressAsync();
        }
        
        public KanaProgress? GetKanaProgress(int kanaId)
        {
            return KanaProgress.FirstOrDefault(kp => kp.KanaId == kanaId);
        }
        
        public bool IsKanaMastered(int kanaId)
        {
            var progress = GetKanaProgress(kanaId);
            return progress?.Mastered ?? false;
        }
        
        public double GetKanaAccuracy(int kanaId)
        {
            var progress = GetKanaProgress(kanaId);
            return progress?.AccuracyRate ?? 0;
        }
    }
}
