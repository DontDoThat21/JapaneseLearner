# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Development Commands

### Build and Run
- `dotnet build` - Build the application
- `dotnet run` - Run the application in development mode
- `dotnet clean` - Clean build artifacts

### Database Operations
- `dotnet ef migrations add <MigrationName>` - Create new database migration
- `dotnet ef database update` - Apply pending migrations to database
- `dotnet ef migrations remove` - Remove the last migration
- `dotnet ef database drop` - Drop the database (use with caution)

## Project Architecture

### Technology Stack
- **Framework**: WPF (.NET 6.0-windows)
- **UI**: MaterialDesignInXamlToolkit with custom Dark Midnight Theme
- **Database**: SQLite with Entity Framework Core
- **Architecture**: MVVM pattern with dependency injection
- **AI Integration**: ChatGPT API for Japanese language assistance

### Core Directory Structure
- **Models/**: Domain entities (Kanji, Vocabulary, Grammar, User, Progress tracking)
- **ViewModels/**: MVVM view models with business logic
- **Views/**: WPF UserControls and Windows with XAML UI
- **Services/**: Business services (DatabaseService, ChatGPTJapaneseService, SRSCalculationService, etc.)
- **Data/**: Entity Framework DbContext and migrations
- **Converters/**: WPF value converters for data binding
- **Commands/**: RelayCommand implementation for MVVM
- **Resources/Themes/**: Custom Material Design theme definitions

### Key Services Architecture
- **DatabaseService**: Central data access layer for all entities
- **ChatGPTJapaneseService**: AI-powered Japanese language explanations
- **SRSCalculationService**: Spaced Repetition System algorithm implementation
- **JLPTService**: JLPT level categorization and progress tracking
- **ReviewQueueService**: Manages review scheduling and queue operations
- **ImportExportService**: Handles data import/export (CSV, Anki, JSON)
- **PerformanceMonitoringService**: Application performance tracking

### Database Schema
The application uses SQLite with these core entities:
- **Users**: User profiles and study streaks
- **Kanji/KanjiProgress**: Character data with individual progress tracking
- **Vocabulary/VocabularyProgress**: Word entries with learning progress
- **Grammar/GrammarProgress**: Grammar patterns by JLPT level
- **StudySessions**: Study session history and statistics
- **ReviewQueue**: SRS-based review scheduling
- **KanaCharacters/KanaProgress**: Hiragana/Katakana mastery tracking

### Navigation and View Management
The MainViewModel handles navigation between views:
- Dashboard, Kanji, Vocabulary, Grammar, Practice, JLPT Progress
- Uses string-based view selection with data template switching
- Each view has its own dedicated ViewModel with specific responsibilities

### Configuration
- **appsettings.json**: Contains database connection, ChatGPT API settings, SRS intervals, and application preferences
- **Environment Variable**: `CHATGPT_API_KEY` required for AI features
- **Material Design**: Custom dark midnight theme with Japanese-inspired colors

### Japanese Language Specific Features
- Furigana rendering support for kanji readings
- Pitch accent visualization
- Stroke order data and animations
- Kanji radical breakdown and search
- JLPT level categorization (N5-N1)
- On/Kun reading practice modes
- Spaced Repetition System tailored for Japanese learning

### Error Handling Patterns
- Services use try-catch with proper exception logging
- ViewModels handle async operations with loading states
- Database operations include transaction rollback on failures
- UI shows user-friendly error messages via MessageBox

## Development Notes

### Adding New Features
1. Create model classes in Models/ directory
2. Add DbSet properties to AppDbContext.cs
3. Create database migration: `dotnet ef migrations add FeatureName`
4. Implement service methods in appropriate service class
5. Create ViewModel with proper data binding
6. Design XAML view with Material Design components
7. Update navigation in MainViewModel if needed

### Working with Japanese Text
- Use UTF-8 encoding for all Japanese text handling
- Store readings as JSON arrays in database (OnReadingsJson, KunReadingsJson)
- Implement proper collation for Japanese text sorting
- Consider furigana placement in UI design

### Material Design Integration
- Use MaterialDesignInXamlToolkit components consistently
- Follow the established color scheme in DarkMidnightTheme.xaml
- Apply JLPT level colors: N5 (Green), N4 (Light Green), N3 (Amber), N2 (Orange), N1 (Red)
- Maintain consistent spacing and elevation patterns