using System;
using System.ComponentModel;

namespace PerAppAudioController.Models
{
    public class AudioSession : INotifyPropertyChanged
    {
        private string _displayName = string.Empty;
        private string _executableName = string.Empty;
        private uint _processId;
        private float _volume = 1.0f;
        private bool _isMuted;

        public string DisplayName
        {
            get => _displayName;
            set
            {
                _displayName = value;
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        public string ExecutableName
        {
            get => _executableName;
            set
            {
                _executableName = value;
                OnPropertyChanged(nameof(ExecutableName));
            }
        }

        public uint ProcessId
        {
            get => _processId;
            set
            {
                _processId = value;
                OnPropertyChanged(nameof(ProcessId));
            }
        }

        public float Volume
        {
            get => _volume;
            set
            {
                _volume = Math.Clamp(value, 0.0f, 1.0f);
                OnPropertyChanged(nameof(Volume));
                OnPropertyChanged(nameof(VolumePercent));
            }
        }

        public int VolumePercent => (int)(_volume * 100);

        public bool IsMuted
        {
            get => _isMuted;
            set
            {
                _isMuted = value;
                OnPropertyChanged(nameof(IsMuted));
            }
        }

        public string SessionKey => $"{ProcessId}_{ExecutableName}";

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}