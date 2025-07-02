using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NAudio.CoreAudioApi;
using PerAppAudioController.Models;

namespace PerAppAudioController.Services
{
    public class AudioService : IDisposable
    {
        private readonly MMDeviceEnumerator _deviceEnumerator;
        private bool _disposed;

        public AudioService()
        {
            _deviceEnumerator = new MMDeviceEnumerator();
        }

        public List<AudioSession> GetAudioSessions()
        {
            var sessions = new List<AudioSession>();

            try
            {
                var devices = _deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
                
                foreach (var device in devices)
                {
                    try
                    {
                        var sessionManager = device.AudioSessionManager;
                        var sessionCollection = sessionManager.Sessions;

                        for (int i = 0; i < sessionCollection.Count; i++)
                        {
                            try
                            {
                                var session = sessionCollection[i];
                                
                                if (session.GetProcessID != 0)
                                {
                                    var audioSession = CreateAudioSession(session);
                                    if (audioSession != null && !sessions.Any(s => s.SessionKey == audioSession.SessionKey))
                                    {
                                        sessions.Add(audioSession);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error processing session {i}: {ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error processing device {device.FriendlyName}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting audio sessions: {ex.Message}");
            }

            return sessions.OrderBy(s => s.DisplayName).ToList();
        }

        private AudioSession? CreateAudioSession(AudioSessionControl sessionControl)
        {
            try
            {
                var processId = sessionControl.GetProcessID;
                var displayName = sessionControl.DisplayName;

                if (string.IsNullOrEmpty(displayName))
                {
                    displayName = GetProcessName(processId) ?? "Unknown";
                }

                var executableName = GetExecutableName(processId) ?? "Unknown.exe";

                return new AudioSession
                {
                    ProcessId = processId,
                    DisplayName = displayName,
                    ExecutableName = executableName,
                    Volume = sessionControl.SimpleAudioVolume.Volume,
                    IsMuted = sessionControl.SimpleAudioVolume.Mute
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating audio session: {ex.Message}");
                return null;
            }
        }

        public bool SetSessionVolume(uint processId, float volume)
        {
            try
            {
                var devices = _deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
                
                foreach (var device in devices)
                {
                    var sessionManager = device.AudioSessionManager;
                    var sessionCollection = sessionManager.Sessions;

                    for (int i = 0; i < sessionCollection.Count; i++)
                    {
                        var session = sessionCollection[i];
                        if (session.GetProcessID == processId)
                        {
                            session.SimpleAudioVolume.Volume = Math.Clamp(volume, 0.0f, 1.0f);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error setting session volume: {ex.Message}");
            }
            return false;
        }

        public bool SetSessionMute(uint processId, bool mute)
        {
            try
            {
                var devices = _deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
                
                foreach (var device in devices)
                {
                    var sessionManager = device.AudioSessionManager;
                    var sessionCollection = sessionManager.Sessions;

                    for (int i = 0; i < sessionCollection.Count; i++)
                    {
                        var session = sessionCollection[i];
                        if (session.GetProcessID == processId)
                        {
                            session.SimpleAudioVolume.Mute = mute;
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error setting session mute: {ex.Message}");
            }
            return false;
        }

        private string? GetProcessName(uint processId)
        {
            try
            {
                var process = Process.GetProcessById((int)processId);
                return process.ProcessName;
            }
            catch
            {
                return null;
            }
        }

        private string? GetExecutableName(uint processId)
        {
            try
            {
                var process = Process.GetProcessById((int)processId);
                return Path.GetFileName(process.MainModule?.FileName);
            }
            catch
            {
                return null;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _deviceEnumerator?.Dispose();
                _disposed = true;
            }
        }
    }
}