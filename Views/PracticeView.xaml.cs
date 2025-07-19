using System.Windows.Controls;
using System.Windows;

namespace JapaneseTracker.Views
{
    /// <summary>
    /// Interaction logic for PracticeView.xaml
    /// </summary>
    public partial class PracticeView : UserControl
    {
        public PracticeView()
        {
            InitializeComponent();
        }
        
        private void ClearWritingCanvas(object sender, RoutedEventArgs e)
        {
            if (FindName("WritingCanvas") is InkCanvas canvas)
            {
                canvas.Strokes.Clear();
            }
        }
    }
}
