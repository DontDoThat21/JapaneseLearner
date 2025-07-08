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
        }
    }
}
