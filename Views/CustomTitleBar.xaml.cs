using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace JapaneseTracker.Views
{
    /// <summary>
    /// Interaction logic for CustomTitleBar.xaml
    /// </summary>
    public partial class CustomTitleBar : UserControl
    {
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

        private Window? _parentWindow;

        public CustomTitleBar()
        {
            InitializeComponent();
            Loaded += CustomTitleBar_Loaded;
        }

        private void CustomTitleBar_Loaded(object sender, RoutedEventArgs e)
        {
            _parentWindow = Window.GetWindow(this);
            if (_parentWindow != null)
            {
                // Set up event handlers
                SetupEventHandlers();
                
                // Update maximize/restore button based on window state
                UpdateMaximizeRestoreButton();
                _parentWindow.StateChanged += ParentWindow_StateChanged;
            }
        }

        private void SetupEventHandlers()
        {
            if (_parentWindow == null) return;

            // Window controls
            MinimizeButton.Click += (s, e) => _parentWindow.WindowState = WindowState.Minimized;
            MaximizeRestoreButton.Click += (s, e) => ToggleMaximizeRestore();
            CloseButton.Click += (s, e) => _parentWindow.Close();

            // Drag functionality
            DragArea.MouseLeftButtonDown += (s, e) => 
            {
                if (e.ClickCount == 2)
                {
                    ToggleMaximizeRestore();
                }
                else if (e.LeftButton == MouseButtonState.Pressed)
                {
                    _parentWindow.DragMove();
                }
            };

            // Action buttons (these can be exposed as events or commands later)
            SearchButton.Click += (s, e) => OnSearchClicked();
            NotificationsButton.Click += (s, e) => OnNotificationsClicked();
            ProfileButton.Click += (s, e) => OnProfileClicked();
        }

        private void ToggleMaximizeRestore()
        {
            if (_parentWindow == null) return;

            _parentWindow.WindowState = _parentWindow.WindowState == WindowState.Maximized 
                ? WindowState.Normal 
                : WindowState.Maximized;
        }

        private void ParentWindow_StateChanged(object? sender, EventArgs e)
        {
            UpdateMaximizeRestoreButton();
        }

        private void UpdateMaximizeRestoreButton()
        {
            if (_parentWindow == null) return;

            var template = MaximizeRestoreButton.Template;
            if (template != null)
            {
                var icon = template.FindName("MaximizeIcon", MaximizeRestoreButton) as MaterialDesignThemes.Wpf.PackIcon;
                if (icon != null)
                {
                    icon.Kind = _parentWindow.WindowState == WindowState.Maximized 
                        ? MaterialDesignThemes.Wpf.PackIconKind.WindowRestore 
                        : MaterialDesignThemes.Wpf.PackIconKind.WindowMaximize;
                }
            }

            MaximizeRestoreButton.ToolTip = _parentWindow.WindowState == WindowState.Maximized 
                ? "Restore" 
                : "Maximize";
        }

        // Events that can be handled by parent or bound to commands
        public event EventHandler? SearchClicked;
        public event EventHandler? NotificationsClicked;
        public event EventHandler? ProfileClicked;

        protected virtual void OnSearchClicked()
        {
            SearchClicked?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnNotificationsClicked()
        {
            NotificationsClicked?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnProfileClicked()
        {
            ProfileClicked?.Invoke(this, EventArgs.Empty);
        }

        // Dependency Properties for customization
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(CustomTitleBar), 
                new PropertyMetadata("Japanese Learner"));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty SubtitleProperty =
            DependencyProperty.Register("Subtitle", typeof(string), typeof(CustomTitleBar), 
                new PropertyMetadata("TODOWIP"));

        public string Subtitle
        {
            get { return (string)GetValue(SubtitleProperty); }
            set { SetValue(SubtitleProperty, value); }
        }
        
        public static readonly DependencyProperty CurrentSectionProperty =
            DependencyProperty.Register("CurrentSection", typeof(string), typeof(CustomTitleBar), 
                new PropertyMetadata("Dashboard"));

        public string CurrentSection
        {
            get { return (string)GetValue(CurrentSectionProperty); }
            set { SetValue(CurrentSectionProperty, value); }
        }
        
        public static readonly DependencyProperty NotificationCountProperty =
            DependencyProperty.Register("NotificationCount", typeof(int), typeof(CustomTitleBar), 
                new PropertyMetadata(0, OnNotificationCountChanged));

        public int NotificationCount
        {
            get { return (int)GetValue(NotificationCountProperty); }
            set { SetValue(NotificationCountProperty, value); }
        }
        
        private static void OnNotificationCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CustomTitleBar titleBar)
            {
                var badge = titleBar.FindName("NotificationBadge") as Border;
                if (badge != null)
                {
                    badge.Visibility = (int)e.NewValue > 0 ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }
    }
}