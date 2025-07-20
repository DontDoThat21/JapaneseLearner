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
        private readonly ImportExportService? _importExportService;
        private readonly PerformanceMonitoringService? _performanceMonitoringService;
        
        private User? _currentUser;
        private string _selectedView = "Dashboard";
        private bool _isLoading = false;
        
        public MainViewModel(
            DatabaseService databaseService,
            ChatGPTJapaneseService chatGPTService,
            JLPTService jlptService,
            ImportExportService? importExportService = null,
            PerformanceMonitoringService? performanceMonitoringService = null)
        {
            _databaseService = databaseService;
            _chatGPTService = chatGPTService;
            _jlptService = jlptService;
            _importExportService = importExportService;
            _performanceMonitoringService = performanceMonitoringService;
            
            NavigateCommand = new RelayCommand<string>(Navigate);
            LoadDataCommand = new RelayCommand(async () => await LoadDataAsync());
            ExportDataCommand = new RelayCommand<string>(async (format) => await ExportDataAsync(format));
            ImportDataCommand = new RelayCommand<string>(async (format) => await ImportDataAsync(format));
            ShowPerformanceReportCommand = new RelayCommand(ShowPerformanceReport);
            ShowKeyboardShortcutsCommand = new RelayCommand(ShowKeyboardShortcuts);
            
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
        public ICommand ExportDataCommand { get; }
        public ICommand ImportDataCommand { get; }
        public ICommand ShowPerformanceReportCommand { get; }
        public ICommand ShowKeyboardShortcutsCommand { get; }
        
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

        #region Phase 5: Import/Export Commands

        private async Task ExportDataAsync(string? format)
        {
            if (_importExportService == null || CurrentUser == null)
            {
                System.Windows.MessageBox.Show("Export service not available or no user loaded.", 
                    "Export Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            IsLoading = true;
            try
            {
                // For demo purposes, we'll use a simple file dialog approach
                var saveDialog = new Microsoft.Win32.SaveFileDialog();
                
                switch (format?.ToLower())
                {
                    case "csv_kanji":
                        saveDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                        saveDialog.FileName = ImportExportService.GetSuggestedExportFilename("Kanji", "csv");
                        if (saveDialog.ShowDialog() == true)
                        {
                            var kanji = await _databaseService.GetKanjiByJLPTLevelAsync("N5"); // Example: export N5 kanji
                            await _importExportService.ExportKanjiToCsvAsync(saveDialog.FileName, kanji);
                            System.Windows.MessageBox.Show("Kanji exported successfully!", "Export Complete");
                        }
                        break;
                        
                    case "anki_kanji":
                        saveDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                        saveDialog.FileName = ImportExportService.GetSuggestedExportFilename("Kanji", "anki");
                        if (saveDialog.ShowDialog() == true)
                        {
                            var kanji = await _databaseService.GetKanjiByJLPTLevelAsync("N5");
                            await _importExportService.ExportKanjiToAnkiAsync(saveDialog.FileName, kanji);
                            System.Windows.MessageBox.Show("Kanji exported to Anki format successfully!", "Export Complete");
                        }
                        break;
                        
                    case "json_backup":
                        saveDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                        saveDialog.FileName = ImportExportService.GetSuggestedExportFilename("UserProgress", "json");
                        if (saveDialog.ShowDialog() == true)
                        {
                            await _importExportService.ExportUserProgressToJsonAsync(saveDialog.FileName, CurrentUser);
                            System.Windows.MessageBox.Show("User progress backup created successfully!", "Export Complete");
                        }
                        break;
                        
                    default:
                        System.Windows.MessageBox.Show("Unsupported export format.", "Export Error");
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Export failed: {ex.Message}", "Export Error", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ImportDataAsync(string? format)
        {
            if (_importExportService == null)
            {
                System.Windows.MessageBox.Show("Import service not available.", 
                    "Import Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            IsLoading = true;
            try
            {
                var openDialog = new Microsoft.Win32.OpenFileDialog();
                
                switch (format?.ToLower())
                {
                    case "csv_kanji":
                        openDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                        if (openDialog.ShowDialog() == true)
                        {
                            var importedKanji = await _importExportService.ImportKanjiFromCsvAsync(openDialog.FileName);
                            System.Windows.MessageBox.Show($"Successfully imported {importedKanji.Count} kanji!", "Import Complete");
                        }
                        break;
                        
                    default:
                        System.Windows.MessageBox.Show("Unsupported import format.", "Import Error");
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Import failed: {ex.Message}", "Import Error", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ShowPerformanceReport()
        {
            if (_performanceMonitoringService == null)
            {
                System.Windows.MessageBox.Show("Performance monitoring service not available.", 
                    "Performance Report", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return;
            }

            var report = _performanceMonitoringService.GeneratePerformanceReport();
            
            // Create a new window to show the performance report
            var reportWindow = new System.Windows.Window
            {
                Title = "Performance Report",
                Width = 800,
                Height = 600,
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner
            };

            var textBox = new System.Windows.Controls.TextBox
            {
                Text = report,
                IsReadOnly = true,
                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                FontSize = 12,
                VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
                Margin = new System.Windows.Thickness(10)
            };

            reportWindow.Content = textBox;
            reportWindow.Show();
        }

        private void ShowKeyboardShortcuts()
        {
            var shortcuts = new[]
            {
                "F1 - Dashboard",
                "F2 - Kanji Study", 
                "F3 - Vocabulary",
                "F4 - Grammar",
                "F5 - Practice Mode",
                "F6 - JLPT Progress",
                "",
                "== Practice Mode ==",
                "Space - Next Card",
                "Tab - Show Answer", 
                "Enter - Submit Answer",
                "H - Show Hint",
                "R - Reload Cards",
                "",
                "== Import/Export ==",
                "Ctrl+E - Export CSV",
                "Ctrl+I - Import CSV",
                "Ctrl+Shift+E - Export to Anki",
                "Ctrl+B - Backup Progress",
                "",
                "== Other ==",
                "Ctrl+F - Search Kanji",
                "Ctrl+Shift+P - Performance Report",
                "F12 - Reload Data", 
                "Escape - Return to Dashboard"
            };

            var helpText = string.Join("\n", shortcuts);
            
            System.Windows.MessageBox.Show(helpText, "Keyboard Shortcuts", 
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        #endregion
    }
}
