using Microsoft.EntityFrameworkCore;
using JapaneseTracker.Models;
using JapaneseTracker.Services;

namespace JapaneseTracker.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        
        // DbSets
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Kanji> Kanji { get; set; } = null!;
        public DbSet<KanjiProgress> KanjiProgress { get; set; } = null!;
        public DbSet<Vocabulary> Vocabulary { get; set; } = null!;
        public DbSet<VocabularyProgress> VocabularyProgress { get; set; } = null!;
        public DbSet<Grammar> Grammar { get; set; } = null!;
        public DbSet<GrammarProgress> GrammarProgress { get; set; } = null!;
        public DbSet<StudySession> StudySessions { get; set; } = null!;
        public DbSet<KanaCharacter> KanaCharacters { get; set; } = null!;
        public DbSet<KanaProgress> KanaProgress { get; set; } = null!;
        
        // Phase 4: Interactive Learning - New DbSets
        public DbSet<ReviewQueue> ReviewQueue { get; set; } = null!;
        public DbSet<ReviewSession> ReviewSessions { get; set; } = null!;
        public DbSet<ReviewResult> ReviewResults { get; set; } = null!;
        public DbSet<SentenceBuildingExercise> SentenceBuildingExercises { get; set; } = null!;
        public DbSet<WritingPracticeSession> WritingPracticeSessions { get; set; } = null!;
        public DbSet<WritingPracticeStroke> WritingPracticeStrokes { get; set; } = null!;
        public DbSet<PitchAccentPattern> PitchAccentPatterns { get; set; } = null!;
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Ignore classes that are only used for JSON serialization
            modelBuilder.Ignore<ExampleSentence>();
            modelBuilder.Ignore<GrammarExample>();
            modelBuilder.Ignore<VocabularyExample>();
            modelBuilder.Ignore<PitchAccentVisualization>();
            modelBuilder.Ignore<PitchAccentPoint>();
            
            // Ignore computed List<string> properties that are deserialized from JSON fields
            // These properties should not be mapped to database tables
            modelBuilder.Entity<Kanji>()
                .Ignore(k => k.OnReadings)
                .Ignore(k => k.KunReadings)
                .Ignore(k => k.Radicals)
                .Ignore(k => k.ExampleWords);
            
            modelBuilder.Entity<Vocabulary>()
                .Ignore(v => v.ExampleSentences)
                .Ignore(v => v.RelatedKanji);
            
            modelBuilder.Entity<Grammar>()
                .Ignore(g => g.Examples)
                .Ignore(g => g.RelatedGrammar);
            
            modelBuilder.Entity<SentenceBuildingExercise>()
                .Ignore(s => s.WordBank)
                .Ignore(s => s.Hints);
            
            // Configure indexes for better performance
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
            
            modelBuilder.Entity<Kanji>()
                .HasIndex(k => k.Character)
                .IsUnique();
            
            modelBuilder.Entity<Kanji>()
                .HasIndex(k => k.JLPTLevel);
            
            modelBuilder.Entity<Vocabulary>()
                .HasIndex(v => v.Word);
            
            modelBuilder.Entity<Vocabulary>()
                .HasIndex(v => v.JLPTLevel);
            
            modelBuilder.Entity<Grammar>()
                .HasIndex(g => g.Pattern);
            
            modelBuilder.Entity<Grammar>()
                .HasIndex(g => g.JLPTLevel);
            
            modelBuilder.Entity<KanaCharacter>()
                .HasIndex(k => k.Character)
                .IsUnique();
            
            modelBuilder.Entity<KanaCharacter>()
                .HasIndex(k => k.Type);
            
            // Configure foreign key relationships
            modelBuilder.Entity<KanjiProgress>()
                .HasOne(kp => kp.User)
                .WithMany(u => u.KanjiProgress)
                .HasForeignKey(kp => kp.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<KanjiProgress>()
                .HasOne(kp => kp.Kanji)
                .WithMany(k => k.ProgressRecords)
                .HasForeignKey(kp => kp.KanjiId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<VocabularyProgress>()
                .HasOne(vp => vp.User)
                .WithMany(u => u.VocabularyProgress)
                .HasForeignKey(vp => vp.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<VocabularyProgress>()
                .HasOne(vp => vp.Vocabulary)
                .WithMany(v => v.ProgressRecords)
                .HasForeignKey(vp => vp.VocabId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<GrammarProgress>()
                .HasOne(gp => gp.User)
                .WithMany(u => u.GrammarProgress)
                .HasForeignKey(gp => gp.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<GrammarProgress>()
                .HasOne(gp => gp.Grammar)
                .WithMany(g => g.ProgressRecords)
                .HasForeignKey(gp => gp.GrammarId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<StudySession>()
                .HasOne(ss => ss.User)
                .WithMany(u => u.StudySessions)
                .HasForeignKey(ss => ss.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<KanaProgress>()
                .HasOne(kp => kp.User)
                .WithMany(u => u.KanaProgress)
                .HasForeignKey(kp => kp.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<KanaProgress>()
                .HasOne(kp => kp.KanaCharacter)
                .WithMany(k => k.ProgressRecords)
                .HasForeignKey(kp => kp.KanaId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Phase 4: Interactive Learning relationships
            modelBuilder.Entity<ReviewQueue>()
                .HasOne(rq => rq.User)
                .WithMany()
                .HasForeignKey(rq => rq.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<ReviewSession>()
                .HasOne(rs => rs.User)
                .WithMany()
                .HasForeignKey(rs => rs.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<ReviewResult>()
                .HasOne(rr => rr.Session)
                .WithMany(s => s.Results)
                .HasForeignKey(rr => rr.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<ReviewResult>()
                .HasOne(rr => rr.QueueItem)
                .WithMany()
                .HasForeignKey(rr => rr.QueueId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<WritingPracticeStroke>()
                .HasOne<WritingPracticeSession>()
                .WithMany(s => s.Strokes)
                .HasForeignKey(s => s.PracticeSessionId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<PitchAccentPattern>()
                .HasOne(p => p.Vocabulary)
                .WithMany()
                .HasForeignKey(p => p.VocabularyId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Seed initial data
            SeedData(modelBuilder);
        }
        
        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Hiragana characters
            var hiraganaChars = new[]
            {
                new { KanaId = 1, Character = "あ", Type = "Hiragana", Romaji = "a", StrokeCount = 3 },
                new { KanaId = 2, Character = "い", Type = "Hiragana", Romaji = "i", StrokeCount = 2 },
                new { KanaId = 3, Character = "う", Type = "Hiragana", Romaji = "u", StrokeCount = 2 },
                new { KanaId = 4, Character = "え", Type = "Hiragana", Romaji = "e", StrokeCount = 2 },
                new { KanaId = 5, Character = "お", Type = "Hiragana", Romaji = "o", StrokeCount = 3 },
                new { KanaId = 6, Character = "か", Type = "Hiragana", Romaji = "ka", StrokeCount = 3 },
                new { KanaId = 7, Character = "き", Type = "Hiragana", Romaji = "ki", StrokeCount = 4 },
                new { KanaId = 8, Character = "く", Type = "Hiragana", Romaji = "ku", StrokeCount = 1 },
                new { KanaId = 9, Character = "け", Type = "Hiragana", Romaji = "ke", StrokeCount = 3 },
                new { KanaId = 10, Character = "こ", Type = "Hiragana", Romaji = "ko", StrokeCount = 2 }
            };
            
            // Seed Katakana characters
            var katakanaChars = new[]
            {
                new { KanaId = 11, Character = "ア", Type = "Katakana", Romaji = "a", StrokeCount = 2 },
                new { KanaId = 12, Character = "イ", Type = "Katakana", Romaji = "i", StrokeCount = 2 },
                new { KanaId = 13, Character = "ウ", Type = "Katakana", Romaji = "u", StrokeCount = 3 },
                new { KanaId = 14, Character = "エ", Type = "Katakana", Romaji = "e", StrokeCount = 3 },
                new { KanaId = 15, Character = "オ", Type = "Katakana", Romaji = "o", StrokeCount = 3 },
                new { KanaId = 16, Character = "カ", Type = "Katakana", Romaji = "ka", StrokeCount = 2 },
                new { KanaId = 17, Character = "キ", Type = "Katakana", Romaji = "ki", StrokeCount = 3 },
                new { KanaId = 18, Character = "ク", Type = "Katakana", Romaji = "ku", StrokeCount = 2 },
                new { KanaId = 19, Character = "ケ", Type = "Katakana", Romaji = "ke", StrokeCount = 3 },
                new { KanaId = 20, Character = "コ", Type = "Katakana", Romaji = "ko", StrokeCount = 2 }
            };
            
            modelBuilder.Entity<KanaCharacter>().HasData(hiraganaChars);
            modelBuilder.Entity<KanaCharacter>().HasData(katakanaChars);
            
            // Seed sample Kanji
            var sampleKanji = new[]
            {
                new { KanjiId = 1, Character = "一", Meaning = "one", OnReadingsJson = "[\"イチ\",\"イツ\"]", KunReadingsJson = "[\"ひと\",\"ひと.つ\"]", StrokeCount = 1, JLPTLevel = "N5", Grade = 1, RadicalsJson = "[\"一\"]", ExampleWordsJson = "[]" },
                new { KanjiId = 2, Character = "二", Meaning = "two", OnReadingsJson = "[\"ニ\"]", KunReadingsJson = "[\"ふた\",\"ふた.つ\"]", StrokeCount = 2, JLPTLevel = "N5", Grade = 1, RadicalsJson = "[\"二\"]", ExampleWordsJson = "[]" },
                new { KanjiId = 3, Character = "三", Meaning = "three", OnReadingsJson = "[\"サン\"]", KunReadingsJson = "[\"み\",\"み.つ\",\"みっ.つ\"]", StrokeCount = 3, JLPTLevel = "N5", Grade = 1, RadicalsJson = "[\"一\"]", ExampleWordsJson = "[]" },
                new { KanjiId = 4, Character = "人", Meaning = "person", OnReadingsJson = "[\"ジン\",\"ニン\"]", KunReadingsJson = "[\"ひと\"]", StrokeCount = 2, JLPTLevel = "N5", Grade = 1, RadicalsJson = "[\"人\"]", ExampleWordsJson = "[]" },
                new { KanjiId = 5, Character = "日", Meaning = "day, sun", OnReadingsJson = "[\"ニチ\",\"ジツ\"]", KunReadingsJson = "[\"ひ\",\"か\"]", StrokeCount = 4, JLPTLevel = "N5", Grade = 1, RadicalsJson = "[\"日\"]", ExampleWordsJson = "[]" }
            };
            
            modelBuilder.Entity<Kanji>().HasData(sampleKanji);
            
            // Seed sample Vocabulary
            var sampleVocabulary = new[]
            {
                new { VocabId = 1, Word = "こんにちは", Reading = "こんにちは", Meaning = "hello, good afternoon", PartOfSpeech = "interjection", JLPTLevel = "N5", PitchAccent = 4, ExampleSentencesJson = "[]", RelatedKanjiJson = "[]" },
                new { VocabId = 2, Word = "ありがとう", Reading = "ありがとう", Meaning = "thank you", PartOfSpeech = "interjection", JLPTLevel = "N5", PitchAccent = 4, ExampleSentencesJson = "[]", RelatedKanjiJson = "[]" },
                new { VocabId = 3, Word = "すみません", Reading = "すみません", Meaning = "excuse me, sorry", PartOfSpeech = "interjection", JLPTLevel = "N5", PitchAccent = 4, ExampleSentencesJson = "[]", RelatedKanjiJson = "[]" },
                new { VocabId = 4, Word = "学生", Reading = "がくせい", Meaning = "student", PartOfSpeech = "noun", JLPTLevel = "N5", PitchAccent = 0, ExampleSentencesJson = "[]", RelatedKanjiJson = "[\"学\",\"生\"]" },
                new { VocabId = 5, Word = "先生", Reading = "せんせい", Meaning = "teacher", PartOfSpeech = "noun", JLPTLevel = "N5", PitchAccent = 3, ExampleSentencesJson = "[]", RelatedKanjiJson = "[\"先\",\"生\"]" },
                new { VocabId = 6, Word = "友達", Reading = "ともだち", Meaning = "friend", PartOfSpeech = "noun", JLPTLevel = "N5", PitchAccent = 2, ExampleSentencesJson = "[]", RelatedKanjiJson = "[\"友\",\"達\"]" },
                new { VocabId = 7, Word = "家族", Reading = "かぞく", Meaning = "family", PartOfSpeech = "noun", JLPTLevel = "N5", PitchAccent = 1, ExampleSentencesJson = "[]", RelatedKanjiJson = "[\"家\",\"族\"]" },
                new { VocabId = 8, Word = "食べる", Reading = "たべる", Meaning = "to eat", PartOfSpeech = "verb", JLPTLevel = "N5", PitchAccent = 2, ExampleSentencesJson = "[]", RelatedKanjiJson = "[\"食\"]" }
            };
            
            modelBuilder.Entity<Vocabulary>().HasData(sampleVocabulary);
            
            // Seed sample Grammar
            var sampleGrammar = new[]
            {
                new { GrammarId = 1, Pattern = "です/である", Meaning = "to be (polite/formal)", Structure = "Noun + です", JLPTLevel = "N5", ExamplesJson = "[]", Notes = "Basic copula for polite speech", RelatedGrammarJson = "[]" },
                new { GrammarId = 2, Pattern = "は (particle)", Meaning = "topic marker", Structure = "Noun + は + predicate", JLPTLevel = "N5", ExamplesJson = "[]", Notes = "Marks the topic of the sentence", RelatedGrammarJson = "[]" },
                new { GrammarId = 3, Pattern = "を (particle)", Meaning = "object marker", Structure = "Noun + を + verb", JLPTLevel = "N5", ExamplesJson = "[]", Notes = "Marks the direct object", RelatedGrammarJson = "[]" },
                new { GrammarId = 4, Pattern = "に (particle)", Meaning = "destination, time, indirect object", Structure = "Noun + に", JLPTLevel = "N5", ExamplesJson = "[]", Notes = "Multiple uses including destination and time", RelatedGrammarJson = "[]" },
                new { GrammarId = 5, Pattern = "で (particle)", Meaning = "location of action, means", Structure = "Noun + で", JLPTLevel = "N5", ExamplesJson = "[]", Notes = "Indicates where an action takes place", RelatedGrammarJson = "[]" },
                new { GrammarId = 6, Pattern = "が (particle)", Meaning = "subject marker", Structure = "Noun + が + predicate", JLPTLevel = "N5", ExamplesJson = "[]", Notes = "Marks the grammatical subject", RelatedGrammarJson = "[]" }
            };
            
            modelBuilder.Entity<Grammar>().HasData(sampleGrammar);
        }
    }
}
