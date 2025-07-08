using Microsoft.EntityFrameworkCore;
using JapaneseTracker.Models;

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
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
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
        }
    }
}
