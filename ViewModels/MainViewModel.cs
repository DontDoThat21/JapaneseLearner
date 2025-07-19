using System;
using JapaneseTracker.Services;
using JapaneseTracker.Models;
using JapaneseTracker.Commands;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JapaneseTracker.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly ChatGPTJapaneseService _chatGPTService;
        private readonly JLPTService _jlptService;
        
        private User? _currentUser;
        private string _selectedView = "Dashboard";
        private bool _isLoading = false;
        
        public MainViewModel(
            DatabaseService databaseService,
            ChatGPTJapaneseService chatGPTService,
            JLPTService jlptService)
        {
            _databaseService = databaseService;
            _chatGPTService = chatGPTService;
            _jlptService = jlptService;
            
            NavigateCommand = new RelayCommand<string>(Navigate);
            LoadDataCommand = new RelayCommand(async () => await LoadDataAsync());
            
            // Initialize with default user for demo
            _ = InitializeDefaultUserAsync();
        }
        
        public User? CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }
        
        public string SelectedView
        {
            get => _selectedView;
            set => SetProperty(ref _selectedView, value);
        }
        
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }
        
        public ObservableCollection<string> NavigationItems { get; } = new()
        {
            "Dashboard",
            "Kanji",
            "Vocabulary",
            "Grammar",
            "Practice",
            "JLPT Progress"
        };
        
        public ICommand NavigateCommand { get; }
        public ICommand LoadDataCommand { get; }
        
        private void Navigate(string? viewName)
        {
            if (!string.IsNullOrEmpty(viewName))
            {
                SelectedView = viewName;
            }
        }
        
        private async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                if (CurrentUser != null)
                {
                    CurrentUser = await _databaseService.GetUserByIdAsync(CurrentUser.UserId);
                }
            }
            catch (Exception ex)
            {
                // Handle error - in a real app, you'd want proper error handling
                System.Diagnostics.Debug.WriteLine($"Error loading data: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private async Task InitializeDefaultUserAsync()
        {
            try
            {
                var user = await _databaseService.GetUserByUsernameAsync("DefaultUser");
                if (user == null)
                {
                    user = await _databaseService.CreateUserAsync("DefaultUser");
                }
                CurrentUser = user;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing user: {ex.Message}");
            }
        }
    }
}
