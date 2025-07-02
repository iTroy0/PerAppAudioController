using System.Windows;
using PerAppAudioController.ViewModels;

namespace PerAppAudioController
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
        }

        private void ExitButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Exit the application completely
            _viewModel?.Dispose();
            System.Windows.Application.Current.Shutdown();
        }

        protected override void OnClosed(System.EventArgs e)
        {
            _viewModel?.Dispose();
            base.OnClosed(e);
        }
    }
}