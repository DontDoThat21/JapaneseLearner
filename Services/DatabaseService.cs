using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JapaneseTracker.Data;
using JapaneseTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace JapaneseTracker.Services
{
    public class DatabaseService
    {
        private readonly AppDbContext _context;
        
        public DatabaseService(AppDbContext context)
        {
            _context = context;
        }
        
        // User operations
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.KanjiProgress)
                .Include(u => u.VocabularyProgress)
                .Include(u => u.GrammarProgress)
                .Include(u => u.StudySessions)
                .Include(u => u.KanaProgress)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }
        
        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);
        }
        
        public async Task<User> CreateUserAsync(string username)
        {
            var user = new User
            {
                Username = username,
                CreatedAt = DateTime.UtcNow,
                LastActive = DateTime.UtcNow
            };
            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }
        
        public async Task UpdateUserAsync(User user)
        {
            user.LastActive = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
        
        // Kanji operations
        public async Task<List<Kanji>> GetKanjiByJLPTLevelAsync(string jlptLevel)
        {
            return await _context.Kanji
                .Where(k => k.JLPTLevel == jlptLevel)
                .OrderBy(k => k.Grade)
                .ThenBy(k => k.StrokeCount)
                .ToListAsync();
        }
        
        public async Task<KanjiProgress?> GetKanjiProgressAsync(int userId, int kanjiId)
        {
            return await _context.KanjiProgress
                .Include(kp => kp.Kanji)
                .FirstOrDefaultAsync(kp => kp.UserId == userId && kp.KanjiId == kanjiId);
        }
        
        public async Task<List<KanjiProgress>> GetKanjiReviewQueueAsync(int userId)
        {
            return await _context.KanjiProgress
                .Include(kp => kp.Kanji)
                .Where(kp => kp.UserId == userId && kp.NextReviewDate <= DateTime.UtcNow)
                .OrderBy(kp => kp.NextReviewDate)
                .ToListAsync();
        }
        
        public async Task UpdateKanjiProgressAsync(KanjiProgress progress)
        {
            _context.KanjiProgress.Update(progress);
            await _context.SaveChangesAsync();
        }
        
        // Vocabulary operations
        public async Task<List<Vocabulary>> GetVocabularyByJLPTLevelAsync(string jlptLevel)
        {
            return await _context.Vocabulary
                .Where(v => v.JLPTLevel == jlptLevel)
                .OrderBy(v => v.Word)
                .ToListAsync();
        }
        
        public async Task<VocabularyProgress?> GetVocabularyProgressAsync(int userId, int vocabId)
        {
            return await _context.VocabularyProgress
                .Include(vp => vp.Vocabulary)
                .FirstOrDefaultAsync(vp => vp.UserId == userId && vp.VocabId == vocabId);
        }
        
        public async Task<List<VocabularyProgress>> GetVocabularyReviewQueueAsync(int userId)
        {
            return await _context.VocabularyProgress
                .Include(vp => vp.Vocabulary)
                .Where(vp => vp.UserId == userId && vp.NextReviewDate <= DateTime.UtcNow)
                .OrderBy(vp => vp.NextReviewDate)
                .ToListAsync();
        }
        
        public async Task UpdateVocabularyProgressAsync(VocabularyProgress progress)
        {
            _context.VocabularyProgress.Update(progress);
            await _context.SaveChangesAsync();
        }
        
        // Grammar operations
        public async Task<List<Grammar>> GetGrammarByJLPTLevelAsync(string jlptLevel)
        {
            return await _context.Grammar
                .Where(g => g.JLPTLevel == jlptLevel)
                .OrderBy(g => g.Pattern)
                .ToListAsync();
        }
        
        public async Task<GrammarProgress?> GetGrammarProgressAsync(int userId, int grammarId)
        {
            return await _context.GrammarProgress
                .Include(gp => gp.Grammar)
                .FirstOrDefaultAsync(gp => gp.UserId == userId && gp.GrammarId == grammarId);
        }
        
        public async Task UpdateGrammarProgressAsync(GrammarProgress progress)
        {
            _context.GrammarProgress.Update(progress);
            await _context.SaveChangesAsync();
        }
        
        // Study session operations
        public async Task<StudySession> CreateStudySessionAsync(int userId, string sessionType)
        {
            var session = new StudySession
            {
                UserId = userId,
                SessionType = sessionType,
                StartTime = DateTime.UtcNow
            };
            
            _context.StudySessions.Add(session);
            await _context.SaveChangesAsync();
            return session;
        }
        
        public async Task UpdateStudySessionAsync(StudySession session)
        {
            _context.StudySessions.Update(session);
            await _context.SaveChangesAsync();
        }
        
        // Kana operations
        public async Task<List<KanaCharacter>> GetKanaCharactersByTypeAsync(string type)
        {
            return await _context.KanaCharacters
                .Where(k => k.Type == type)
                .OrderBy(k => k.Character)
                .ToListAsync();
        }
        
        public async Task<List<KanaProgress>> GetKanaProgressAsync(int userId, string? type = null)
        {
            var query = _context.KanaProgress
                .Include(kp => kp.KanaCharacter)
                .Where(kp => kp.UserId == userId);
            
            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(kp => kp.KanaCharacter.Type == type);
            }
            
            return await query.ToListAsync();
        }
        
        public async Task UpdateKanaProgressAsync(KanaProgress progress)
        {
            _context.KanaProgress.Update(progress);
            await _context.SaveChangesAsync();
        }
        
        // Statistics
        public async Task<Dictionary<string, int>> GetStudyStatisticsAsync(int userId)
        {
            var stats = new Dictionary<string, int>();
            
            stats["TotalKanji"] = await _context.KanjiProgress
                .CountAsync(kp => kp.UserId == userId);
            
            stats["LearnedKanji"] = await _context.KanjiProgress
                .CountAsync(kp => kp.UserId == userId && kp.SRSLevel > 0);
            
            stats["TotalVocabulary"] = await _context.VocabularyProgress
                .CountAsync(vp => vp.UserId == userId);
            
            stats["LearnedVocabulary"] = await _context.VocabularyProgress
                .CountAsync(vp => vp.UserId == userId && vp.SRSLevel > 0);
            
            stats["TotalGrammar"] = await _context.GrammarProgress
                .CountAsync(gp => gp.UserId == userId);
            
            stats["LearnedGrammar"] = await _context.GrammarProgress
                .CountAsync(gp => gp.UserId == userId && gp.UnderstandingLevel > 0);
            
            return stats;
        }
    }
}
