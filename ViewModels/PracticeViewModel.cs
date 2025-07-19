using System;
using JapaneseTracker.Services;
using JapaneseTracker.Models;
using JapaneseTracker.Commands;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;

namespace JapaneseTracker.ViewModels
{
    public class PracticeViewModel : BaseViewModel
    {
        private readonly DatabaseService? _databaseService;
        private readonly SRSCalculationService? _srsService;
        
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
        
        // Parameterless constructor for XAML
        public PracticeViewModel()
        {
            InitializeCommands();
            KanaTypes = new ObservableCollection<string> { "Hiragana", "Katakana" };
        }
        
        public PracticeViewModel(
            DatabaseService databaseService,
            SRSCalculationService srsService) : this()
        {
            _databaseService = databaseService;
            _srsService = srsService;
            
            // Initialize
            _ = InitializeAsync();
        }
        
        private void InitializeCommands()
        {
            LoadKanaCommand = new RelayCommand(async () => await LoadKanaAsync());
            NextKanaCommand = new RelayCommand(NextKana);
            CheckAnswerCommand = new RelayCommand(CheckAnswer);
            SubmitAnswerCommand = new RelayCommand(async () => await SubmitAnswerAsync());
            ShowAnswerCommand = new RelayCommand(() => ShowAnswer = true);
            StartPracticeCommand = new RelayCommand(async () => await StartPracticeAsync());
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
        
        public double AccuracyRate
        {
            get => TotalAnswers > 0 ? (double)CorrectAnswers / TotalAnswers * 100 : 0;
        }
        
        public ObservableCollection<string> KanaTypes { get; set; }
        
        // Commands
        public ICommand LoadKanaCommand { get; private set; }
        public ICommand NextKanaCommand { get; private set; }
        public ICommand CheckAnswerCommand { get; private set; }
        public ICommand SubmitAnswerCommand { get; private set; }
        public ICommand ShowAnswerCommand { get; private set; }
        public ICommand StartPracticeCommand { get; private set; }
        
        private async Task InitializeAsync()
        {
            // Placeholder initialization
            await Task.CompletedTask;
        }
        
        private async Task LoadKanaAsync()
        {
            // Placeholder - load kana characters based on selected type
            await Task.CompletedTask;
        }
        
        private void NextKana()
        {
            // Placeholder - move to next kana character
            ShowAnswer = false;
            UserInput = "";
            CurrentIndex++;
        }
        
        private void CheckAnswer()
        {
            // Placeholder - check if user input matches current kana
            if (CurrentKana != null)
            {
                bool isCorrect = string.Equals(UserInput?.Trim(), CurrentKana.Romaji, StringComparison.OrdinalIgnoreCase);
                if (isCorrect)
                {
                    CorrectAnswers++;
                }
                TotalAnswers++;
                OnPropertyChanged(nameof(AccuracyRate));
                ShowAnswer = true;
            }
        }
        
        private async Task SubmitAnswerAsync()
        {
            // Placeholder - submit answer and move to next
            await Task.CompletedTask;
            NextKana();
        }
        
        private async Task StartPracticeAsync()
        {
            // Placeholder - start practice session
            await LoadKanaAsync();
        }
    }
}
