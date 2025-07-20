using System;
using JapaneseTracker.Services;
using JapaneseTracker.Models;
using JapaneseTracker.Commands;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Linq;

namespace JapaneseTracker.ViewModels
{
    public class PracticeViewModel : BaseViewModel
    {
        private readonly DatabaseService? _databaseService;
        private readonly SRSCalculationService? _srsService;
        private readonly IAudioPronunciationService? _audioService;
        private readonly IPitchAccentService? _pitchAccentService;
        private readonly IReviewQueueService? _reviewQueueService;
        
        private ObservableCollection<KanaCharacter> _kanaCharacters = new();
        private ObservableCollection<KanaProgress> _kanaProgress = new();
        private ObservableCollection<SentenceBuildingExercise> _sentenceExercises = new();
        private ObservableCollection<Vocabulary> _vocabularyItems = new();
        private KanaCharacter? _currentKana;
        private SentenceBuildingExercise? _currentExercise;
        private Vocabulary? _currentVocabulary;
        private string _selectedKanaType = "Hiragana";
        private string _selectedPracticeMode = "Kana Recognition";
        private string _userInput = "";
        private bool _showAnswer = false;
        private bool _isLoading = false;
        private bool _showPitchAccent = false;
        private bool _isWritingMode = false;
        private User? _currentUser;
        private int _currentIndex = 0;
        private int _correctAnswers = 0;
        private int _totalAnswers = 0;
        private PitchAccentVisualization? _currentPitchVisualization;
        private ObservableCollection<string> _sentenceWordBank = new();
        private ObservableCollection<string> _userSentenceConstruction = new();
        
        // Parameterless constructor for XAML
        public PracticeViewModel()
        {
            InitializeCommands();
            KanaTypes = new ObservableCollection<string> { "Hiragana", "Katakana" };
            PracticeModes = new ObservableCollection<string> 
            { 
                "Kana Recognition", 
                "Writing Practice", 
                "Vocabulary", 
                "Sentence Building",
                "Audio Practice"
            };
        }
        
        public PracticeViewModel(
            DatabaseService databaseService,
            SRSCalculationService srsService,
            IAudioPronunciationService audioService,
            IPitchAccentService pitchAccentService,
            IReviewQueueService reviewQueueService) : this()
        {
            _databaseService = databaseService;
            _srsService = srsService;
            _audioService = audioService;
            _pitchAccentService = pitchAccentService;
            _reviewQueueService = reviewQueueService;
            
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
            PlayAudioCommand = new RelayCommand(async () => await PlayAudioAsync());
            TogglePitchAccentCommand = new RelayCommand(() => ShowPitchAccent = !ShowPitchAccent);
            StartWritingCommand = new RelayCommand(() => IsWritingMode = true);
            LoadSentenceExerciseCommand = new RelayCommand(async () => await LoadSentenceExerciseAsync());
            AddWordToSentenceCommand = new RelayCommand<string>(AddWordToSentence);
            RemoveWordFromSentenceCommand = new RelayCommand<int>(RemoveWordFromSentence);
            CheckSentenceCommand = new RelayCommand(CheckSentenceAnswer);
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
        public ObservableCollection<string> PracticeModes { get; set; }
        
        public string SelectedPracticeMode
        {
            get => _selectedPracticeMode;
            set 
            { 
                if (SetProperty(ref _selectedPracticeMode, value))
                {
                    _ = LoadPracticeModeAsync();
                }
            }
        }
        
        public bool ShowPitchAccent
        {
            get => _showPitchAccent;
            set => SetProperty(ref _showPitchAccent, value);
        }
        
        public bool IsWritingMode
        {
            get => _isWritingMode;
            set => SetProperty(ref _isWritingMode, value);
        }
        
        public PitchAccentVisualization? CurrentPitchVisualization
        {
            get => _currentPitchVisualization;
            set => SetProperty(ref _currentPitchVisualization, value);
        }
        
        public SentenceBuildingExercise? CurrentExercise
        {
            get => _currentExercise;
            set => SetProperty(ref _currentExercise, value);
        }
        
        public Vocabulary? CurrentVocabulary
        {
            get => _currentVocabulary;
            set => SetProperty(ref _currentVocabulary, value);
        }
        
        public ObservableCollection<string> SentenceWordBank
        {
            get => _sentenceWordBank;
            set => SetProperty(ref _sentenceWordBank, value);
        }
        
        public ObservableCollection<string> UserSentenceConstruction
        {
            get => _userSentenceConstruction;
            set => SetProperty(ref _userSentenceConstruction, value);
        }
        
        // Commands
        public ICommand LoadKanaCommand { get; private set; }
        public ICommand NextKanaCommand { get; private set; }
        public ICommand CheckAnswerCommand { get; private set; }
        public ICommand SubmitAnswerCommand { get; private set; }
        public ICommand ShowAnswerCommand { get; private set; }
        public ICommand StartPracticeCommand { get; private set; }
        public ICommand PlayAudioCommand { get; private set; }
        public ICommand TogglePitchAccentCommand { get; private set; }
        public ICommand StartWritingCommand { get; private set; }
        public ICommand LoadSentenceExerciseCommand { get; private set; }
        public ICommand AddWordToSentenceCommand { get; private set; }
        public ICommand RemoveWordFromSentenceCommand { get; private set; }
        public ICommand CheckSentenceCommand { get; private set; }
        
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
            // Start practice session based on selected mode
            switch (SelectedPracticeMode)
            {
                case "Kana Recognition":
                    await LoadKanaAsync();
                    break;
                case "Vocabulary":
                    await LoadVocabularyAsync();
                    break;
                case "Sentence Building":
                    await LoadSentenceExerciseAsync();
                    break;
                case "Writing Practice":
                    IsWritingMode = true;
                    await LoadKanaAsync();
                    break;
                case "Audio Practice":
                    await LoadVocabularyAsync();
                    break;
            }
        }
        
        private async Task LoadPracticeModeAsync()
        {
            // Load content based on selected practice mode
            await Task.CompletedTask;
        }
        
        private async Task LoadVocabularyAsync()
        {
            // Placeholder - load vocabulary items
            await Task.CompletedTask;
        }
        
        private async Task LoadSentenceExerciseAsync()
        {
            // Placeholder - load sentence building exercises
            await Task.CompletedTask;
        }
        
        private async Task PlayAudioAsync()
        {
            if (_audioService != null && CurrentVocabulary != null)
            {
                await _audioService.PlayPronunciationAsync(CurrentVocabulary.Reading);
            }
            else if (_audioService != null && CurrentKana != null)
            {
                await _audioService.PlayPronunciationAsync(CurrentKana.Character);
            }
        }
        
        private void AddWordToSentence(string? word)
        {
            if (!string.IsNullOrEmpty(word))
            {
                UserSentenceConstruction.Add(word);
            }
        }
        
        private void RemoveWordFromSentence(int index)
        {
            if (index >= 0 && index < UserSentenceConstruction.Count)
            {
                UserSentenceConstruction.RemoveAt(index);
            }
        }
        
        private void CheckSentenceAnswer()
        {
            if (CurrentExercise != null)
            {
                var userSentence = string.Join("", UserSentenceConstruction);
                var targetSentence = CurrentExercise.TargetSentence.Replace(" ", "");
                
                bool isCorrect = string.Equals(userSentence, targetSentence, StringComparison.OrdinalIgnoreCase);
                if (isCorrect)
                {
                    CorrectAnswers++;
                }
                TotalAnswers++;
                OnPropertyChanged(nameof(AccuracyRate));
                ShowAnswer = true;
            }
        }
        
        private void LoadPitchAccentVisualization()
        {
            if (_pitchAccentService != null && CurrentVocabulary != null)
            {
                // Determine pattern type from pitch accent value
                var patternType = _pitchAccentService.DetectPatternType(
                    CurrentVocabulary.PitchAccent, 
                    CurrentVocabulary?.Reading?.Length ?? 0);
                CurrentPitchVisualization = _pitchAccentService.GenerateVisualization(
                    CurrentVocabulary.Reading, 
                    CurrentVocabulary.PitchAccent, 
                    patternType);
            }
        }
    }
}
