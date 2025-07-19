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
    public class GrammarViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly ChatGPTJapaneseService _chatGPTService;
        
        private ObservableCollection<Grammar> _grammarList = new();
        private ObservableCollection<GrammarProgress> _grammarProgress = new();
        private Grammar? _selectedGrammar;
        private string _selectedJLPTLevel = "N5";
        private string _searchText = "";
        private bool _isLoading = false;
        private User? _currentUser;
        
        public GrammarViewModel(
            DatabaseService databaseService,
            ChatGPTJapaneseService chatGPTService)
        {
            _databaseService = databaseService;
            _chatGPTService = chatGPTService;
            
            // Commands
            LoadGrammarCommand = new RelayCommand(async () => await LoadGrammarAsync());
            SelectGrammarCommand = new RelayCommand<Grammar>(SelectGrammar);
            StudyGrammarCommand = new RelayCommand<Grammar>(async (grammar) => await StudyGrammarAsync(grammar));
            ExplainGrammarCommand = new RelayCommand<Grammar>(async (grammar) => await ExplainGrammarAsync(grammar));
            SearchGrammarCommand = new RelayCommand(async () => await SearchGrammarAsync());
            
            JLPTLevels = new ObservableCollection<string> { "N5", "N4", "N3", "N2", "N1" };
            
            // Initialize
            _ = InitializeAsync();
        }
        
        public ObservableCollection<Grammar> GrammarList
        {
            get => _grammarList;
            set => SetProperty(ref _grammarList, value);
        }
        
        public ObservableCollection<GrammarProgress> GrammarProgress
        {
            get => _grammarProgress;
            set => SetProperty(ref _grammarProgress, value);
        }
        
        public Grammar? SelectedGrammar
        {
            get => _selectedGrammar;
            set => SetProperty(ref _selectedGrammar, value);
        }
        
        public string SelectedJLPTLevel
        {
            get => _selectedJLPTLevel;
            set 
            { 
                if (SetProperty(ref _selectedJLPTLevel, value))
                {
                    _ = LoadGrammarAsync();
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
        
        public ICommand LoadGrammarCommand { get; }
        public ICommand SelectGrammarCommand { get; }
        public ICommand StudyGrammarCommand { get; }
        public ICommand ExplainGrammarCommand { get; }
        public ICommand SearchGrammarCommand { get; }
        
        private async Task InitializeAsync()
        {
            CurrentUser = await _databaseService.GetUserByUsernameAsync("DefaultUser");
            if (CurrentUser != null)
            {
                await LoadGrammarAsync();
                await LoadGrammarProgressAsync();
            }
        }
        
        private async Task LoadGrammarAsync()
        {
            IsLoading = true;
            try
            {
                var grammarList = await _databaseService.GetGrammarByJLPTLevelAsync(SelectedJLPTLevel);
                GrammarList = new ObservableCollection<Grammar>(grammarList);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading grammar: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private async Task LoadGrammarProgressAsync()
        {
            if (CurrentUser == null) return;
            
            try
            {
                var progressList = new List<GrammarProgress>();
                foreach (var grammar in GrammarList)
                {
                    var progress = await _databaseService.GetGrammarProgressAsync(CurrentUser.UserId, grammar.GrammarId);
                    if (progress != null)
                    {
                        progressList.Add(progress);
                    }
                }
                GrammarProgress = new ObservableCollection<GrammarProgress>(progressList);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading grammar progress: {ex.Message}");
            }
        }
        
        private void SelectGrammar(Grammar? grammar)
        {
            SelectedGrammar = grammar;
        }
        
        private async Task StudyGrammarAsync(Grammar? grammar)
        {
            if (grammar == null || CurrentUser == null) return;
            
            try
            {
                // Get or create progress record
                var progress = await _databaseService.GetGrammarProgressAsync(CurrentUser.UserId, grammar.GrammarId);
                if (progress == null)
                {
                    progress = new GrammarProgress
                    {
                        UserId = CurrentUser.UserId,
                        GrammarId = grammar.GrammarId,
                        UnderstandingLevel = 0
                    };
                }
                
                // For now, simulate study progress (in a real app, this would be based on user interaction)
                progress.PracticeCount++;
                progress.LastPracticed = DateTime.UtcNow;
                
                // Increase understanding level gradually
                if (progress.UnderstandingLevel < 100)
                {
                    progress.UnderstandingLevel = Math.Min(100, progress.UnderstandingLevel + 10);
                }
                
                await _databaseService.UpdateGrammarProgressAsync(progress);
                await LoadGrammarProgressAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error studying grammar: {ex.Message}");
            }
        }
        
        private async Task ExplainGrammarAsync(Grammar? grammar)
        {
            if (grammar == null) return;
            
            try
            {
                var explanation = await _chatGPTService.ExplainGrammarPatternAsync(grammar.Pattern, grammar.Meaning);
                // In a real app, you would display this in a dialog or dedicated view
                System.Diagnostics.Debug.WriteLine($"Explanation for {grammar.Pattern}: {explanation}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting explanation: {ex.Message}");
            }
        }
        
        private async Task SearchGrammarAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText)) return;
            
            IsLoading = true;
            try
            {
                var allGrammar = await _databaseService.GetGrammarByJLPTLevelAsync(SelectedJLPTLevel);
                var filteredGrammar = allGrammar.Where(g => 
                    g.Pattern.Contains(SearchText) ||
                    g.Meaning.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    g.Structure.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                ).ToList();
                
                GrammarList = new ObservableCollection<Grammar>(filteredGrammar);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error searching grammar: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        public GrammarProgress? GetGrammarProgress(int grammarId)
        {
            return GrammarProgress.FirstOrDefault(gp => gp.GrammarId == grammarId);
        }
        
        public string GetGrammarMasteryLevel(int grammarId)
        {
            var progress = GetGrammarProgress(grammarId);
            if (progress == null) return "New";
            
            return progress.MasteryLevel;
        }
    }
}
