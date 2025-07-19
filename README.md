# 🌸 Japanese Learning Tracker

A comprehensive WPF desktop application designed to track Japanese language learning progress from absolute beginner to expert level (JLPT N5 → N1). Features a modern Material Design interface with a dark midnight theme, SQLite database for persistence, and ChatGPT API integration for AI-powered learning assistance.

![Japanese Learning Tracker](https://img.shields.io/badge/WPF-.NET%206.0+-blue) ![Material Design](https://img.shields.io/badge/UI-Material%20Design-purple) ![SQLite](https://img.shields.io/badge/Database-SQLite-green) ![ChatGPT](https://img.shields.io/badge/AI-ChatGPT%20API-orange)

## 🎯 Project Goals

- ✨ Create an aesthetically pleasing Japanese learning tracker with Material Design principles
- 📝 Track progress across all Japanese writing systems (Hiragana, Katakana, Kanji)
- 📊 Implement real-time progress tracking for vocabulary, grammar, and JLPT levels
- 💾 Store learning data persistently using SQLite
- 🤖 Integrate ChatGPT API for Japanese language explanations and practice
- 🎓 Provide a smooth progression path from beginner to JLPT N1

## 🛠️ Technology Stack

- **Framework**: WPF (.NET 6.0+)
- **UI Library**: MaterialDesignInXamlToolkit
- **Database**: SQLite with Entity Framework Core
- **API Integration**: OpenAI ChatGPT API
- **Architecture**: MVVM (Model-View-ViewModel)
- **Additional Libraries**:
  - Prism.Core (MVVM support)
  - LiveCharts2 (Progress visualization)
  - Newtonsoft.Json (API communication)
  - WanaKana.NET (Romaji/Kana conversion)

## 🏗️ Application Architecture

### Project Structure
```
JapaneseTracker/
├── App.xaml
├── App.xaml.cs
├── Models/
│   ├── Kanji.cs
│   ├── Vocabulary.cs
│   ├── Grammar.cs
│   ├── KanaCharacter.cs
│   ├── StudySession.cs
│   ├── Progress.cs
│   └── User.cs
├── ViewModels/
│   ├── MainViewModel.cs
│   ├── DashboardViewModel.cs
│   ├── KanjiViewModel.cs
│   ├── VocabularyViewModel.cs
│   ├── GrammarViewModel.cs
│   ├── PracticeViewModel.cs
│   └── JLPTProgressViewModel.cs
├── Views/
│   ├── MainWindow.xaml
│   ├── DashboardView.xaml
│   ├── KanjiView.xaml
│   ├── VocabularyView.xaml
│   ├── GrammarView.xaml
│   ├── PracticeView.xaml
│   └── JLPTProgressView.xaml
├── Services/
│   ├── DatabaseService.cs
│   ├── ChatGPTJapaneseService.cs
│   ├── SRSCalculationService.cs
│   ├── JLPTService.cs
│   └── KanjiRadicalService.cs
├── Data/
│   ├── AppDbContext.cs
│   └── Migrations/
├── Converters/
│   ├── JLPTLevelToColorConverter.cs
│   └── SRSLevelToIconConverter.cs
└── Resources/
    ├── Fonts/
    │   └── NotoSansJP-Regular.otf
    └── Themes/
        └── DarkMidnightTheme.xaml
```

## 📊 Database Schema

### Core Tables
- **Users**: User profiles and study streaks
- **Kanji**: Character data with readings, meanings, and JLPT levels
- **KanjiProgress**: Individual progress tracking with SRS levels
- **Vocabulary**: Word entries with furigana and example sentences
- **VocabularyProgress**: Vocabulary learning progress
- **Grammar**: Grammar patterns by JLPT level
- **GrammarProgress**: Grammar understanding tracking
- **StudySessions**: Study session history and statistics
- **KanaProgress**: Hiragana/Katakana mastery tracking

## 🌟 Core Features

### 📈 Dashboard
- Daily study streak tracker
- JLPT progress overview (N5 → N1)
- Kanji/Vocabulary/Grammar statistics
- SRS review queue counts
- Study time heatmap
- Weekly/Monthly progress charts
- ChatGPT Japanese tip of the day

### 📝 Kanji Learning
- Stroke order animations
- Radical breakdown
- Mnemonic creation and storage
- On/Kun reading practice
- Writing practice with stroke detection
- Related vocabulary display
- JLPT level filtering
- Kanji search by radical/stroke count

### 🗃️ Vocabulary Management
- Word cards with furigana
- Audio pronunciation (via API)
- Pitch accent visualization
- Example sentences with translations
- Context-based learning
- Related kanji highlighting
- Part of speech categorization

### 📚 Grammar Tracking
- Pattern explanation with examples
- Interactive grammar exercises
- Conjugation practice
- Usage notes and exceptions
- JLPT-aligned progression
- ChatGPT grammar explanations

### 🧠 SRS (Spaced Repetition System)
- Customizable review intervals
- Multiple review types:
  - Kanji → Meaning
  - Meaning → Kanji
  - Reading → Meaning
  - Listening → Meaning
- Review statistics
- Difficult items flagging

### 🎮 Practice Modes
- **Hiragana/Katakana**: Character recognition and writing
- **Kanji Writing**: Stroke order practice
- **Reading Comprehension**: Short passages with questions
- **Listening Practice**: ChatGPT-generated audio
- **Sentence Building**: Grammar pattern practice

### 🎯 JLPT Preparation
- Level-specific study plans
- Mock test sections
- Progress tracking per JLPT section
- Vocabulary lists by level
- Grammar patterns by level
- Kanji requirements tracking

## 🎨 UI/UX Design

### Dark Midnight Theme Colors
- **Primary**: `#1A237E` (Deep Indigo)
- **Secondary**: `#E91E63` (Japanese Pink - Sakura inspired)
- **Background**: `#121212`
- **Surface**: `#1E1E1E`
- **Success**: `#4CAF50`
- **Warning**: `#FF9800`
- **Error**: `#CF6679`

### JLPT Level Colors
- **N5**: `#4CAF50` (Green - Beginner)
- **N4**: `#8BC34A` (Light Green)
- **N3**: `#FFC107` (Amber)
- **N2**: `#FF9800` (Orange)
- **N1**: `#F44336` (Red - Advanced)

## 🚀 Implementation Plan

### Phase 1: Foundation (Week 1)
- [x] Setup WPF project with .NET 6.0
- [x] Install and configure MaterialDesignInXamlToolkit
- [x] Create dark midnight theme with Japanese aesthetics
- [x] Setup SQLite with EF Core
- [x] Import Japanese fonts (Noto Sans JP)
- [x] Create database with Japanese-specific tables
- [x] Implement basic MVVM structure

### Phase 2: Core Japanese Features (Week 2-3)
- [x] Build Hiragana/Katakana modules
- [x] Implement Kanji data model and views
- [x] Create vocabulary management system
- [x] Add basic SRS algorithm
- [x] Implement furigana rendering
- [x] Create study session tracking

### Phase 3: Advanced Features (Week 4)
- [x] Integrate ChatGPT API for Japanese
- [ ] Add stroke order animations
- [x] Implement grammar pattern system
- [x] Create JLPT progress tracking
- [x] Add kanji radical search
- [x] Build practice modes

### Phase 4: Interactive Learning (Week 5)
- [ ] Add writing practice with canvas
- [ ] Implement audio pronunciation
- [ ] Create sentence building exercises
- [x] Add pitch accent visualization
- [ ] Build review queue system

### Phase 5: Polish and Optimization (Week 6)
- [ ] Refine animations and transitions
- [ ] Add keyboard shortcuts for reviews
- [x] Implement comprehensive error handling
- [x] Optimize database queries
- [ ] Add data import/export (Anki, CSV)
- [ ] Performance testing with large datasets

## ⚙️ Configuration

### Environment Variables
```
CHATGPT_API_KEY=your-api-key-here
```

### App Settings (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=japanese_tracker.db"
  },
  "ChatGPT": {
    "Model": "gpt-4",
    "MaxTokens": 300,
    "Temperature": 0.7,
    "SystemPrompt": "You are a helpful Japanese language tutor. Provide explanations in English with Japanese examples."
  },
  "SRS": {
    "Intervals": [1, 3, 7, 14, 30, 90, 180, 365],
    "InitialInterval": 1,
    "EasyBonus": 1.3,
    "HardPenalty": 0.6
  },
  "Application": {
    "AutoSaveInterval": 30,
    "DefaultStudyTime": 25,
    "EnableFurigana": true,
    "ShowPitchAccent": true
  }
}
```

## 🧪 Testing Strategy

### Unit Tests
- SRS algorithm calculations
- Japanese text parsing
- Furigana generation
- JLPT level categorization

### Integration Tests
- Database operations with Japanese text
- ChatGPT API responses
- Audio generation
- Import/export functionality

### UI Tests
- Japanese input methods
- Character rendering
- Stroke order animations
- Flashcard interactions

## 🔮 Future Enhancements

### Advanced Input Methods
- Handwriting recognition
- Voice input for speaking practice
- Kanji component search

### Gamification
- XP system for consistent study
- Achievements for milestones
- Study challenges
- Leaderboards

### Content Expansion
- Manga reading assistant
- News article analyzer
- Anime subtitle study mode
- Business Japanese module

### AI Enhancements
- Personalized study recommendations
- Weak point analysis
- Custom exercise generation
- Conversation practice with ChatGPT

## 📚 Resources

- [MaterialDesignInXamlToolkit Documentation](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit)
- [JLPT Resources](https://www.jlpt.jp/e/)
- [Kanji Database](https://www.kanjidatabase.com/)
- [OpenAI API Documentation](https://platform.openai.com/docs)
- [WanaKana.NET](https://github.com/WanaKana/WanaKana.NET)

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

*Made with ❤️ for Japanese language learners*
