using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using JapaneseTracker.Models;
using Microsoft.Extensions.Logging;

namespace JapaneseTracker.Services
{
    /// <summary>
    /// Service for importing and exporting Japanese learning data to/from various formats
    /// Supports CSV and Anki deck formats for Phase 5 implementation
    /// </summary>
    public class ImportExportService
    {
        private readonly DatabaseService _databaseService;
        private readonly ILogger<ImportExportService> _logger;

        public ImportExportService(DatabaseService databaseService, ILogger<ImportExportService> logger)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region CSV Export/Import

        /// <summary>
        /// Export kanji data to CSV format
        /// </summary>
        public async Task<bool> ExportKanjiToCsvAsync(string filePath, List<Kanji> kanjiList)
        {
            try
            {
                _logger.LogInformation("Exporting {Count} kanji to CSV: {FilePath}", kanjiList.Count, filePath);

                var csv = new StringBuilder();
                // CSV Header
                csv.AppendLine("Character,OnReadings,KunReadings,Meaning,JLPTLevel,StrokeCount,Grade");

                foreach (var kanji in kanjiList)
                {
                    var onReadingsString = string.Join(";", kanji.OnReadings);
                    var kunReadingsString = string.Join(";", kanji.KunReadings);
                    
                    csv.AppendLine($"\"{kanji.Character}\"," +
                                  $"\"{onReadingsString}\"," +
                                  $"\"{kunReadingsString}\"," +
                                  $"\"{kanji.Meaning}\"," +
                                  $"\"{kanji.JLPTLevel}\"," +
                                  $"{kanji.StrokeCount}," +
                                  $"{kanji.Grade}");
                }

                await File.WriteAllTextAsync(filePath, csv.ToString(), Encoding.UTF8);
                _logger.LogInformation("Successfully exported kanji to CSV");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export kanji to CSV: {FilePath}", filePath);
                return false;
            }
        }

        /// <summary>
        /// Export vocabulary data to CSV format
        /// </summary>
        public async Task<bool> ExportVocabularyToCsvAsync(string filePath, List<Vocabulary> vocabularyList)
        {
            try
            {
                _logger.LogInformation("Exporting {Count} vocabulary items to CSV: {FilePath}", vocabularyList.Count, filePath);

                var csv = new StringBuilder();
                // CSV Header
                csv.AppendLine("Word,Reading,Meaning,PartOfSpeech,JLPTLevel,ExampleSentences");

                foreach (var vocab in vocabularyList)
                {
                    var examplesString = string.Join("|", vocab.ExampleSentences.Select(ex => $"{ex.Japanese}~{ex.Reading}~{ex.English}"));
                    
                    csv.AppendLine($"\"{vocab.Word}\"," +
                                  $"\"{vocab.Reading}\"," +
                                  $"\"{vocab.Meaning}\"," +
                                  $"\"{vocab.PartOfSpeech}\"," +
                                  $"\"{vocab.JLPTLevel}\"," +
                                  $"\"{examplesString}\"");
                }

                await File.WriteAllTextAsync(filePath, csv.ToString(), Encoding.UTF8);
                _logger.LogInformation("Successfully exported vocabulary to CSV");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export vocabulary to CSV: {FilePath}", filePath);
                return false;
            }
        }

        /// <summary>
        /// Import kanji from CSV format
        /// </summary>
        public async Task<List<Kanji>> ImportKanjiFromCsvAsync(string filePath)
        {
            var importedKanji = new List<Kanji>();

            try
            {
                _logger.LogInformation("Importing kanji from CSV: {FilePath}", filePath);

                var lines = await File.ReadAllLinesAsync(filePath, Encoding.UTF8);
                
                if (lines.Length < 2) // Header + at least one data row
                {
                    _logger.LogWarning("CSV file is empty or missing data: {FilePath}", filePath);
                    return importedKanji;
                }

                // Skip header (first line)
                for (int i = 1; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    var parts = ParseCsvLine(line);
                    if (parts.Length >= 7)
                    {
                        var kanji = new Kanji
                        {
                            Character = parts[0],
                            OnReadings = parts[1].Split(';', StringSplitOptions.RemoveEmptyEntries).ToList(),
                            KunReadings = parts[2].Split(';', StringSplitOptions.RemoveEmptyEntries).ToList(),
                            Meaning = parts[3],
                            JLPTLevel = parts[4],
                            StrokeCount = int.TryParse(parts[5], out int strokes) ? strokes : 0,
                            Grade = int.TryParse(parts[6], out int grade) ? grade : 0
                        };

                        importedKanji.Add(kanji);
                    }
                }

                _logger.LogInformation("Successfully imported {Count} kanji from CSV", importedKanji.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import kanji from CSV: {FilePath}", filePath);
            }

            return importedKanji;
        }

        #endregion

        #region Anki Export

        /// <summary>
        /// Export kanji data to Anki deck format (TSV for easy import into Anki)
        /// </summary>
        public async Task<bool> ExportKanjiToAnkiAsync(string filePath, List<Kanji> kanjiList)
        {
            try
            {
                _logger.LogInformation("Exporting {Count} kanji to Anki format: {FilePath}", kanjiList.Count, filePath);

                var ankiData = new StringBuilder();
                
                foreach (var kanji in kanjiList)
                {
                    // Format: Front (Kanji) [TAB] Back (Reading + Meaning)
                    var front = kanji.Character;
                    var onReadingsString = string.Join(", ", kanji.OnReadings);
                    var kunReadingsString = string.Join(", ", kanji.KunReadings);
                    
                    var back = $"<b>Readings:</b><br/>On: {onReadingsString}<br/>Kun: {kunReadingsString}<br/><br/>" +
                              $"<b>Meaning:</b><br/>{kanji.Meaning}<br/><br/>" +
                              $"<b>JLPT Level:</b> {kanji.JLPTLevel}<br/>" +
                              $"<b>Stroke Count:</b> {kanji.StrokeCount}";

                    ankiData.AppendLine($"{front}\t{back}");
                }

                await File.WriteAllTextAsync(filePath, ankiData.ToString(), Encoding.UTF8);
                _logger.LogInformation("Successfully exported kanji to Anki format");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export kanji to Anki format: {FilePath}", filePath);
                return false;
            }
        }

        /// <summary>
        /// Export vocabulary data to Anki deck format (TSV for easy import into Anki)
        /// </summary>
        public async Task<bool> ExportVocabularyToAnkiAsync(string filePath, List<Vocabulary> vocabularyList)
        {
            try
            {
                _logger.LogInformation("Exporting {Count} vocabulary items to Anki format: {FilePath}", vocabularyList.Count, filePath);

                var ankiData = new StringBuilder();
                
                foreach (var vocab in vocabularyList)
                {
                    // Format: Front (Word) [TAB] Back (Reading + Meaning + Example)
                    var front = vocab.Word;
                    var back = $"<b>Reading:</b><br/>{vocab.Reading}<br/><br/>" +
                              $"<b>Meaning:</b><br/>{vocab.Meaning}<br/><br/>" +
                              $"<b>Part of Speech:</b> {vocab.PartOfSpeech}<br/>" +
                              $"<b>JLPT Level:</b> {vocab.JLPTLevel}";

                    if (vocab.ExampleSentences.Any())
                    {
                        back += "<br/><br/><b>Examples:</b><br/>";
                        foreach (var example in vocab.ExampleSentences.Take(3)) // Limit to 3 examples for readability
                        {
                            back += $"{example.Japanese}<br/>";
                            if (!string.IsNullOrEmpty(example.Reading))
                            {
                                back += $"<i>{example.Reading}</i><br/>";
                            }
                            back += $"{example.English}<br/><br/>";
                        }
                    }

                    ankiData.AppendLine($"{front}\t{back}");
                }

                await File.WriteAllTextAsync(filePath, ankiData.ToString(), Encoding.UTF8);
                _logger.LogInformation("Successfully exported vocabulary to Anki format");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export vocabulary to Anki format: {FilePath}", filePath);
                return false;
            }
        }

        #endregion

        #region JSON Export/Import (for full app data backup)

        /// <summary>
        /// Export all user progress data to JSON format for backup
        /// </summary>
        public async Task<bool> ExportUserProgressToJsonAsync(string filePath, User user)
        {
            try
            {
                _logger.LogInformation("Exporting user progress to JSON: {FilePath}", filePath);

                // Get user progress data from database
                var kanjiProgress = await _databaseService.GetKanjiProgressByUserAsync(user.UserId);
                var vocabularyProgress = await _databaseService.GetVocabularyProgressByUserAsync(user.UserId);
                var grammarProgress = await _databaseService.GetGrammarProgressByUserAsync(user.UserId);
                var studySessions = await _databaseService.GetStudySessionsByUserAsync(user.UserId);

                var exportData = new
                {
                    ExportDate = DateTime.UtcNow,
                    User = user,
                    KanjiProgress = kanjiProgress,
                    VocabularyProgress = vocabularyProgress,
                    GrammarProgress = grammarProgress,
                    StudySessions = studySessions.TakeLast(50).ToList() // Last 50 sessions to keep file size reasonable
                };

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(exportData, options);
                await File.WriteAllTextAsync(filePath, json, Encoding.UTF8);

                _logger.LogInformation("Successfully exported user progress to JSON");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export user progress to JSON: {FilePath}", filePath);
                return false;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Parse a CSV line handling quoted fields properly
        /// </summary>
        private static string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"' && (i == 0 || line[i - 1] != '\\'))
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.ToString().Trim('"'));
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            result.Add(current.ToString().Trim('"'));
            return result.ToArray();
        }

        /// <summary>
        /// Get suggested export filename based on data type and current date
        /// </summary>
        public static string GetSuggestedExportFilename(string dataType, string format)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var extension = format.ToLower() switch
            {
                "csv" => "csv",
                "anki" => "txt",
                "json" => "json",
                _ => "txt"
            };

            return $"JapaneseTracker_{dataType}_{timestamp}.{extension}";
        }

        #endregion
    }
}