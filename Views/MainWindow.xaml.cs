using System.Windows;
using JapaneseTracker.ViewModels;

namespace JapaneseTracker.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // Set the DataContext using the service locator
            DataContext = App.GetService<MainViewModel>();
            
            // Handle custom title bar events
            SetupTitleBarEvents();
        }
        
        private void SetupTitleBarEvents()
        {
            // Get reference to the CustomTitleBar
            if (CustomTitleBar != null)
            {
                CustomTitleBar.SearchClicked += (s, e) => HandleSearchClicked();
                CustomTitleBar.NotificationsClicked += (s, e) => HandleNotificationsClicked();
                CustomTitleBar.ProfileClicked += (s, e) => HandleProfileClicked();
            }
        }
        
        private void HandleSearchClicked()
        {
            // Handle search button click - could execute search command from ViewModel
            if (DataContext is MainViewModel mainViewModel)
            {
                // Example: trigger search functionality
                // mainViewModel.ShowSearchCommand?.Execute(null);
            }
        }
        
        private void HandleNotificationsClicked()
        {
            // Handle notifications button click
            // Could show notifications dialog or panel
        }
        
        private void HandleProfileClicked()
        {
            // Handle profile button click
            // Could show user profile dialog or settings
        }
    }
}
