using System;
using System.Collections.Generic;
using System.Linq;
using JapaneseTracker.Models;

namespace JapaneseTracker.Services
{
    public class KanjiRadicalService
    {
        private readonly Dictionary<string, RadicalInfo> _radicals;
        
        public KanjiRadicalService()
        {
            _radicals = InitializeRadicals();
        }
        
        private Dictionary<string, RadicalInfo> InitializeRadicals()
        {
            return new Dictionary<string, RadicalInfo>
            {
                ["人"] = new RadicalInfo
                {
                    Radical = "人",
                    Meaning = "person",
                    StrokeCount = 2,
                    Position = RadicalPosition.Left,
                    Variants = new[] { "亻" },
                    Description = "Represents a person or human being"
                },
                ["水"] = new RadicalInfo
                {
                    Radical = "水",
                    Meaning = "water",
                    StrokeCount = 4,
                    Position = RadicalPosition.Left,
                    Variants = new[] { "氵", "氺" },
                    Description = "Represents water or liquid"
                },
                ["火"] = new RadicalInfo
                {
                    Radical = "火",
                    Meaning = "fire",
                    StrokeCount = 4,
                    Position = RadicalPosition.Bottom,
                    Variants = new[] { "灬" },
                    Description = "Represents fire or heat"
                },
                ["木"] = new RadicalInfo
                {
                    Radical = "木",
                    Meaning = "tree, wood",
                    StrokeCount = 4,
                    Position = RadicalPosition.Left,
                    Variants = new[] { "朩" },
                    Description = "Represents tree, wood, or plant"
                },
                ["金"] = new RadicalInfo
                {
                    Radical = "金",
                    Meaning = "metal, gold",
                    StrokeCount = 8,
                    Position = RadicalPosition.Left,
                    Variants = new[] { "釒" },
                    Description = "Represents metal or precious things"
                },
                ["土"] = new RadicalInfo
                {
                    Radical = "土",
                    Meaning = "earth, soil",
                    StrokeCount = 3,
                    Position = RadicalPosition.Left,
                    Variants = new string[0],
                    Description = "Represents earth or ground"
                },
                ["日"] = new RadicalInfo
                {
                    Radical = "日",
                    Meaning = "sun, day",
                    StrokeCount = 4,
                    Position = RadicalPosition.Left,
                    Variants = new string[0],
                    Description = "Represents sun or day"
                },
                ["月"] = new RadicalInfo
                {
                    Radical = "月",
                    Meaning = "moon, month",
                    StrokeCount = 4,
                    Position = RadicalPosition.Left,
                    Variants = new string[0],
                    Description = "Represents moon or month"
                },
                ["心"] = new RadicalInfo
                {
                    Radical = "心",
                    Meaning = "heart, mind",
                    StrokeCount = 4,
                    Position = RadicalPosition.Bottom,
                    Variants = new[] { "忄", "㣺" },
                    Description = "Represents heart, mind, or emotions"
                },
                ["手"] = new RadicalInfo
                {
                    Radical = "手",
                    Meaning = "hand",
                    StrokeCount = 4,
                    Position = RadicalPosition.Left,
                    Variants = new[] { "扌", "龵" },
                    Description = "Represents hand or action"
                },
                ["口"] = new RadicalInfo
                {
                    Radical = "口",
                    Meaning = "mouth",
                    StrokeCount = 3,
                    Position = RadicalPosition.Left,
                    Variants = new string[0],
                    Description = "Represents mouth or opening"
                },
                ["田"] = new RadicalInfo
                {
                    Radical = "田",
                    Meaning = "field",
                    StrokeCount = 5,
                    Position = RadicalPosition.Left,
                    Variants = new string[0],
                    Description = "Represents rice field or farming"
                },
                ["目"] = new RadicalInfo
                {
                    Radical = "目",
                    Meaning = "eye",
                    StrokeCount = 5,
                    Position = RadicalPosition.Left,
                    Variants = new string[0],
                    Description = "Represents eye or seeing"
                },
                ["糸"] = new RadicalInfo
                {
                    Radical = "糸",
                    Meaning = "thread, silk",
                    StrokeCount = 6,
                    Position = RadicalPosition.Left,
                    Variants = new[] { "糹" },
                    Description = "Represents thread or textile"
                },
                ["言"] = new RadicalInfo
                {
                    Radical = "言",
                    Meaning = "word, speech",
                    StrokeCount = 7,
                    Position = RadicalPosition.Left,
                    Variants = new[] { "訁" },
                    Description = "Represents speech or language"
                }
            };
        }
        
        public RadicalInfo? GetRadicalInfo(string radical)
        {
            return _radicals.ContainsKey(radical) ? _radicals[radical] : null;
        }
        
        public List<RadicalInfo> GetAllRadicals()
        {
            return _radicals.Values.OrderBy(r => r.StrokeCount).ToList();
        }
        
        public List<RadicalInfo> GetRadicalsByStrokeCount(int strokeCount)
        {
            return _radicals.Values
                .Where(r => r.StrokeCount == strokeCount)
                .OrderBy(r => r.Radical)
                .ToList();
        }
        
        public List<RadicalInfo> GetRadicalsByPosition(RadicalPosition position)
        {
            return _radicals.Values
                .Where(r => r.Position == position)
                .OrderBy(r => r.StrokeCount)
                .ToList();
        }
        
        public List<string> ExtractRadicalsFromKanji(string kanji)
        {
            // This is a simplified implementation
            // In a real application, you would need a comprehensive kanji decomposition database
            var foundRadicals = new List<string>();
            
            foreach (var radical in _radicals.Keys)
            {
                if (kanji.Contains(radical))
                {
                    foundRadicals.Add(radical);
                }
            }
            
            return foundRadicals;
        }
        
        public List<string> FindKanjiByRadical(string radical, List<Kanji> kanjiList)
        {
            var result = new List<string>();
            
            foreach (var kanji in kanjiList)
            {
                if (kanji.Radicals.Contains(radical))
                {
                    result.Add(kanji.Character);
                }
            }
            
            return result;
        }
        
        public string GenerateRadicalMnemonic(string radical, string meaning)
        {
            var radicalInfo = GetRadicalInfo(radical);
            if (radicalInfo == null)
            {
                return $"Remember: {radical} means {meaning}";
            }
            
            // Generate simple mnemonics based on radical meaning
            return radicalInfo.Meaning switch
            {
                "person" => $"Think of {radical} as a person standing - it helps remember {meaning}",
                "water" => $"The {radical} radical flows like water, connecting to {meaning}",
                "fire" => $"The {radical} radical burns bright like fire, relating to {meaning}",
                "tree" => $"The {radical} radical grows like a tree, branching into {meaning}",
                "hand" => $"The {radical} radical reaches out like a hand toward {meaning}",
                "heart" => $"The {radical} radical beats like a heart, feeling {meaning}",
                "mouth" => $"The {radical} radical speaks like a mouth, saying {meaning}",
                "eye" => $"The {radical} radical sees like an eye, viewing {meaning}",
                _ => $"The {radical} radical ({radicalInfo.Meaning}) connects to {meaning}"
            };
        }
        
        public List<RadicalInfo> GetSimilarRadicals(string radical)
        {
            var radicalInfo = GetRadicalInfo(radical);
            if (radicalInfo == null)
            {
                return new List<RadicalInfo>();
            }
            
            return _radicals.Values
                .Where(r => r.Radical != radical && 
                           (r.StrokeCount == radicalInfo.StrokeCount || 
                            r.Position == radicalInfo.Position))
                .OrderBy(r => r.StrokeCount)
                .ToList();
        }
        
        public RadicalSearchResult SearchRadicals(string query)
        {
            var result = new RadicalSearchResult();
            
            // Search by radical character
            var exactMatch = _radicals.Values.FirstOrDefault(r => r.Radical == query);
            if (exactMatch != null)
            {
                result.ExactMatches.Add(exactMatch);
            }
            
            // Search by meaning
            var meaningMatches = _radicals.Values
                .Where(r => r.Meaning.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();
            result.MeaningMatches.AddRange(meaningMatches);
            
            // Search by variants
            var variantMatches = _radicals.Values
                .Where(r => r.Variants.Contains(query))
                .ToList();
            result.VariantMatches.AddRange(variantMatches);
            
            return result;
        }
    }
    
    public class RadicalInfo
    {
        public string Radical { get; set; } = string.Empty;
        public string Meaning { get; set; } = string.Empty;
        public int StrokeCount { get; set; }
        public RadicalPosition Position { get; set; }
        public string[] Variants { get; set; } = Array.Empty<string>();
        public string Description { get; set; } = string.Empty;
    }
    
    public class RadicalSearchResult
    {
        public List<RadicalInfo> ExactMatches { get; set; } = new();
        public List<RadicalInfo> MeaningMatches { get; set; } = new();
        public List<RadicalInfo> VariantMatches { get; set; } = new();
    }
    
    public enum RadicalPosition
    {
        Left,
        Right,
        Top,
        Bottom,
        Enclosure,
        Corner
    }
}
