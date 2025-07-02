using System;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;

namespace PerAppAudioController
{
    public partial class App : Application
    {
        private TaskbarIcon? _notifyIcon;
        private MainWindow? _mainWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // Create the notification icon
                _notifyIcon = new TaskbarIcon
                {
                    ToolTipText = "Per-App Audio Controller"
                };

                // Try to load icon, but don't crash if it fails
                try
                {
                    _notifyIcon.IconSource = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/icon.ico"));
                }
                catch
                {
                    // Icon failed to load, app will use default
                }

                // Set up context menu
                _notifyIcon.ContextMenu = new System.Windows.Controls.ContextMenu();
                
                var showMenuItem = new System.Windows.Controls.MenuItem { Header = "Show" };
                showMenuItem.Click += (s, args) => ShowMainWindow();
                _notifyIcon.ContextMenu.Items.Add(showMenuItem);
                
                _notifyIcon.ContextMenu.Items.Add(new System.Windows.Controls.Separator());
                
                var exitMenuItem = new System.Windows.Controls.MenuItem { Header = "Exit Application" };
                exitMenuItem.Click += (s, args) => ExitApplication();
                _notifyIcon.ContextMenu.Items.Add(exitMenuItem);

                // Double-click to show
                _notifyIcon.TrayMouseDoubleClick += (s, args) => ShowMainWindow();

                // Show window on first launch
                ShowMainWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting application: {ex.Message}", "Per-App Audio Controller", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        private void ShowMainWindow()
        {
            if (_mainWindow == null)
            {
                _mainWindow = new MainWindow();
                _mainWindow.Closing += (s, e) =>
                {
                    e.Cancel = true;
                    _mainWindow.Hide();
                    _notifyIcon?.ShowBalloonTip("Per-App Audio Controller", "Running in background", BalloonIcon.Info);
                };
            }

            _mainWindow.Show();
            _mainWindow.WindowState = WindowState.Normal;
            _mainWindow.Activate();
        }

        private void ExitApplication()
        {
            _mainWindow?.Close();
            _notifyIcon?.Dispose();
            Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _notifyIcon?.Dispose();
            base.OnExit(e);
        }
    }
}