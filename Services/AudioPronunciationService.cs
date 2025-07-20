using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace JapaneseTracker.Services
{
    public interface IAudioPronunciationService
    {
        Task<bool> PlayPronunciationAsync(string text, string language = "ja-JP");
        Task<string> GenerateAudioFileAsync(string text, string language = "ja-JP");
        Task<bool> PlayAudioFileAsync(string filePath);
        bool IsAudioSupported();
        List<string> GetAvailableVoices();
    }
    
    public class AudioPronunciationService : IAudioPronunciationService, IDisposable
    {
        private readonly SpeechSynthesizer _synthesizer;
        private readonly HttpClient _httpClient;
        private readonly ILogger<AudioPronunciationService> _logger;
        private readonly string _audioDirectory;
        private bool _disposed = false;
        
        public AudioPronunciationService(IConfiguration configuration, ILogger<AudioPronunciationService> logger)
        {
            _synthesizer = new SpeechSynthesizer();
            _httpClient = new HttpClient();
            _logger = logger;
            _audioDirectory = Path.Combine(Path.GetTempPath(), "JapaneseTracker", "Audio");
            
            // Ensure audio directory exists
            Directory.CreateDirectory(_audioDirectory);
            
            // Configure synthesizer for Japanese
            ConfigureSynthesizer();
        }
        
        private void ConfigureSynthesizer()
        {
            try
            {
                // Try to set Japanese voice
                var japaneseVoices = new[] { "Microsoft Haruka Desktop", "Microsoft Sayaka Desktop", "Microsoft Ichiro Desktop" };
                
                foreach (var voiceName in japaneseVoices)
                {
                    try
                    {
                        _synthesizer.SelectVoice(voiceName);
                        _logger.LogInformation($"Selected voice: {voiceName}");
                        break;
                    }
                    catch (ArgumentException)
                    {
                        // Voice not available, try next
                        continue;
                    }
                }
                
                // Configure speech rate and volume
                _synthesizer.Rate = 0; // Normal speed
                _synthesizer.Volume = 80;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to configure Japanese voice: {ex.Message}");
            }
        }
        
        public async Task<bool> PlayPronunciationAsync(string text, string language = "ja-JP")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(text))
                    return false;
                
                // Clean the text for pronunciation
                var cleanText = CleanTextForPronunciation(text);
                
                await Task.Run(() =>
                {
                    _synthesizer.SpeakAsync(cleanText);
                });
                
                _logger.LogDebug($"Playing pronunciation for: {cleanText}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to play pronunciation for: {text}");
                return false;
            }
        }
        
        public async Task<string> GenerateAudioFileAsync(string text, string language = "ja-JP")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(text))
                    return string.Empty;
                
                var cleanText = CleanTextForPronunciation(text);
                var fileName = $"{GetHashCode(cleanText)}_{DateTime.UtcNow.Ticks}.wav";
                var filePath = Path.Combine(_audioDirectory, fileName);
                
                await Task.Run(() =>
                {
                    _synthesizer.SetOutputToWaveFile(filePath);
                    _synthesizer.Speak(cleanText);
                    _synthesizer.SetOutputToDefaultAudioDevice();
                });
                
                _logger.LogDebug($"Generated audio file: {filePath}");
                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to generate audio file for: {text}");
                return string.Empty;
            }
        }
        
        public async Task<bool> PlayAudioFileAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning($"Audio file not found: {filePath}");
                    return false;
                }
                
                // Use media player to play the audio file
                await Task.Run(() =>
                {
                    var player = new System.Media.SoundPlayer(filePath);
                    player.Play();
                });
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to play audio file: {filePath}");
                return false;
            }
        }
        
        public bool IsAudioSupported()
        {
            try
            {
                // Check if speech synthesis is available
                return _synthesizer != null && _synthesizer.GetInstalledVoices().Count > 0;
            }
            catch
            {
                return false;
            }
        }
        
        public List<string> GetAvailableVoices()
        {
            var voices = new List<string>();
            
            try
            {
                foreach (var voice in _synthesizer.GetInstalledVoices())
                {
                    if (voice.Enabled)
                    {
                        voices.Add(voice.VoiceInfo.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get available voices");
            }
            
            return voices;
        }
        
        private string CleanTextForPronunciation(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            
            // Remove furigana notation like 漢字(かんじ)
            var cleaned = System.Text.RegularExpressions.Regex.Replace(text, @"\([^)]*\)", "");
            
            // Remove any remaining brackets or special characters that might interfere
            cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"[【】『』「」〈〉《》]", "");
            
            return cleaned.Trim();
        }
        
        private string ComputeSHA256Hash(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(text);
                var hashBytes = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }
        
        // Alternative method using online TTS service (placeholder for future implementation)
        public async Task<bool> PlayOnlinePronunciationAsync(string text, string language = "ja-JP")
        {
            try
            {
                // This would integrate with services like Google TTS, Azure Cognitive Services, etc.
                // For now, fall back to local synthesis
                return await PlayPronunciationAsync(text, language);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Online pronunciation service failed");
                return false;
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _synthesizer?.Dispose();
                _httpClient?.Dispose();
                _disposed = true;
            }
        }
    }
}