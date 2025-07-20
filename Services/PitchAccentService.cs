using System;
using System.Collections.Generic;
using System.Linq;
using JapaneseTracker.Models;
using Microsoft.Extensions.Logging;

namespace JapaneseTracker.Services
{
    public interface IPitchAccentService
    {
        PitchAccentVisualization GenerateVisualization(string word, int accentPosition, string patternType);
        string GetPatternDescription(string patternType, int accentPosition);
        List<PitchAccentPoint> GeneratePitchCurve(string word, int accentPosition, string patternType);
        bool IsValidAccentPosition(string word, int accentPosition);
        string DetectPatternType(int accentPosition, int wordLength);
    }
    
    public class PitchAccentService : IPitchAccentService
    {
        private readonly ILogger<PitchAccentService> _logger;
        
        public PitchAccentService(ILogger<PitchAccentService> logger)
        {
            _logger = logger;
        }
        
        public PitchAccentVisualization GenerateVisualization(string word, int accentPosition, string patternType)
        {
            try
            {
                var morae = ExtractMorae(word);
                var pitchCurve = GeneratePitchCurve(word, accentPosition, patternType);
                
                var visualization = new PitchAccentVisualization
                {
                    Word = word,
                    PatternType = patternType,
                    AccentPosition = accentPosition,
                    Morae = morae,
                    PitchPoints = pitchCurve,
                    Description = GetPatternDescription(patternType, accentPosition),
                    SVGPath = GenerateSVGPath(pitchCurve)
                };
                
                _logger.LogDebug($"Generated pitch accent visualization for: {word}");
                return visualization;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to generate pitch accent visualization for: {word}");
                return new PitchAccentVisualization { Word = word, Description = "Error generating visualization" };
            }
        }
        
        public string GetPatternDescription(string patternType, int accentPosition)
        {
            return patternType.ToUpper() switch
            {
                "HEIBAN" => "平板 (Flat) - Low start, high plateau, stays high",
                "ATAMA" => "頭高 (Head High) - High start, drops after first mora",
                "NAKADAKA" => $"中高 (Mid High) - Low start, high middle, drops after mora {accentPosition}",
                "ODAKA" => "尾高 (Tail High) - Low start, rises to high, drops on particle",
                _ => "Unknown pattern"
            };
        }
        
        public List<PitchAccentPoint> GeneratePitchCurve(string word, int accentPosition, string patternType)
        {
            var morae = ExtractMorae(word);
            var points = new List<PitchAccentPoint>();
            
            try
            {
                for (int i = 0; i < morae.Count; i++)
                {
                    var pitch = CalculatePitchForMora(i, accentPosition, patternType, morae.Count);
                    
                    points.Add(new PitchAccentPoint
                    {
                        Position = i,
                        Mora = morae[i],
                        PitchLevel = pitch,
                        IsAccented = (accentPosition > 0 && i == accentPosition - 1)
                    });
                }
                
                return points;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to generate pitch curve for: {word}");
                return new List<PitchAccentPoint>();
            }
        }
        
        public bool IsValidAccentPosition(string word, int accentPosition)
        {
            if (accentPosition < 0)
                return false;
            
            var morae = ExtractMorae(word);
            
            // 0 is valid (heiban), position should not exceed word length
            return accentPosition <= morae.Count;
        }
        
        public string DetectPatternType(int accentPosition, int wordLength)
        {
            if (accentPosition == 0)
                return "HeiBan";
            if (accentPosition == 1)
                return "AtaMa";
            if (accentPosition == wordLength)
                return "ODaka";
            
            return "NaKaDaka";
        }
        
        private List<string> ExtractMorae(string word)
        {
            var morae = new List<string>();
            
            for (int i = 0; i < word.Length; i++)
            {
                var currentChar = word[i];
                
                // Check if current character is followed by a small kana (っ, ゃ, ゅ, ょ, etc.)
                if (i + 1 < word.Length)
                {
                    var nextChar = word[i + 1];
                    if (IsSmallKana(nextChar))
                    {
                        morae.Add(word.Substring(i, 2));
                        i++; // Skip the small kana
                        continue;
                    }
                }
                
                // Check for long vowel marks (ー)
                if (currentChar == 'ー' && morae.Count > 0)
                {
                    morae[morae.Count - 1] += currentChar;
                    continue;
                }
                
                morae.Add(currentChar.ToString());
            }
            
            return morae;
        }
        
        private bool IsSmallKana(char character)
        {
            // Small hiragana and katakana characters
            var smallKana = new[] { 'っ', 'ゃ', 'ゅ', 'ょ', 'ァ', 'ィ', 'ゥ', 'ェ', 'ォ', 'ッ', 'ャ', 'ュ', 'ョ', 'ヮ' };
            return smallKana.Contains(character);
        }
        
        private double CalculatePitchForMora(int position, int accentPosition, string patternType, int totalLength)
        {
            const double LOW_PITCH = 0.3;
            const double HIGH_PITCH = 0.8;
            
            return patternType.ToUpper() switch
            {
                "HEIBAN" => position == 0 ? LOW_PITCH : HIGH_PITCH,
                "ATAMA" => position == 0 ? HIGH_PITCH : LOW_PITCH,
                "NAKADAKA" => GetNakaDakaPitch(position, accentPosition, LOW_PITCH, HIGH_PITCH),
                "ODAKA" => position < totalLength - 1 ? (position == 0 ? LOW_PITCH : HIGH_PITCH) : LOW_PITCH,
                _ => 0.5 // Default middle pitch
            };
        }
        
        private double GetNakaDakaPitch(int position, int accentPosition, double lowPitch, double highPitch)
        {
            if (position == 0)
                return lowPitch;
            if (position < accentPosition)
                return highPitch;
            
            return lowPitch;
        }
        
        private string GenerateSVGPath(List<PitchAccentPoint> pitchPoints)
        {
            if (pitchPoints.Count == 0)
                return "";
            
            const int WIDTH = 300;
            const int HEIGHT = 100;
            const int MARGIN = 20;
            
            var pathBuilder = System.Text.StringBuilder();
            pathBuilder.Append("M ");
            
            for (int i = 0; i < pitchPoints.Count; i++)
            {
                var x = MARGIN + (i * (WIDTH - 2 * MARGIN) / Math.Max(1, pitchPoints.Count - 1));
                var y = HEIGHT - MARGIN - (pitchPoints[i].PitchLevel * (HEIGHT - 2 * MARGIN));
                
                if (i == 0)
                    pathBuilder.Append($"{x},{y}");
                else
                    pathBuilder.Append($" L {x},{y}");
            }
            
            return pathBuilder.ToString();
        }
    }
    
    public class PitchAccentVisualization
    {
        public string Word { get; set; } = string.Empty;
        public string PatternType { get; set; } = string.Empty;
        public int AccentPosition { get; set; }
        public List<string> Morae { get; set; } = new List<string>();
        public List<PitchAccentPoint> PitchPoints { get; set; } = new List<PitchAccentPoint>();
        public string Description { get; set; } = string.Empty;
        public string SVGPath { get; set; } = string.Empty;
    }
    
    public class PitchAccentPoint
    {
        public int Position { get; set; }
        public string Mora { get; set; } = string.Empty;
        public double PitchLevel { get; set; } // 0.0 to 1.0
        public bool IsAccented { get; set; }
    }
}