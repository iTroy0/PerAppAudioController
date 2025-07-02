using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using PerAppAudioController.Models;
using PerAppAudioController.Services;

namespace PerAppAudioController.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly AudioService _audioService;
        private readonly DispatcherTimer _refreshTimer;
        private bool _isRefreshing;

        public ObservableCollection<AudioSession> AudioSessions { get; }
        public ICommand RefreshCommand { get; }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                _isRefreshing = value;
                OnPropertyChanged(nameof(IsRefreshing));
            }
        }

        public MainViewModel()
        {
            _audioService = new AudioService();
            AudioSessions = new ObservableCollection<AudioSession>();

            RefreshCommand = new RelayCommand(async () => await RefreshDataAsync());

            // Auto-refresh every 5 seconds
            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _refreshTimer.Tick += async (s, e) => await RefreshDataAsync();
            _refreshTimer.Start();

            // Initial load
            Task.Run(async () => await RefreshDataAsync());
        }

        private async Task RefreshDataAsync()
        {
            if (IsRefreshing) return;

            IsRefreshing = true;

            try
            {
                await Task.Run(() =>
                {
                    var sessions = _audioService.GetAudioSessions();

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        // Update existing sessions or add new ones
                        foreach (var session in sessions)
                        {
                            var existingSession = AudioSessions.FirstOrDefault(s => s.SessionKey == session.SessionKey);
                            if (existingSession != null)
                            {
                                // Update values without replacing the object
                                existingSession.Volume = session.Volume;
                                existingSession.IsMuted = session.IsMuted;
                            }
                            else
                            {
                                // Subscribe to property changes for new sessions
                                session.PropertyChanged += OnSessionPropertyChanged;
                                AudioSessions.Add(session);
                            }
                        }

                        // Remove sessions that no longer exist
                        var sessionsToRemove = AudioSessions
                            .Where(s => !sessions.Any(ns => ns.SessionKey == s.SessionKey))
                            .ToList();

                        foreach (var session in sessionsToRemove)
                        {
                            session.PropertyChanged -= OnSessionPropertyChanged;
                            AudioSessions.Remove(session);
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing data: {ex.Message}");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private void OnSessionPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is AudioSession session)
            {
                switch (e.PropertyName)
                {
                    case nameof(AudioSession.Volume):
                        _audioService.SetSessionVolume(session.ProcessId, session.Volume);
                        break;
                    case nameof(AudioSession.IsMuted):
                        _audioService.SetSessionMute(session.ProcessId, session.IsMuted);
                        break;
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            _refreshTimer?.Stop();
            
            foreach (var session in AudioSessions)
            {
                session.PropertyChanged -= OnSessionPropertyChanged;
            }
            
            _audioService?.Dispose();
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
        public void Execute(object? parameter) => _execute();
    }
}