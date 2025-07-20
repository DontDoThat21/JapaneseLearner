using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Windows;
using JapaneseTracker.Data;
using JapaneseTracker.Services;
using JapaneseTracker.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace JapaneseTracker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IHost? _host;

        protected override void OnStartup(StartupEventArgs e)
        {
            // Initialize services BEFORE calling base.OnStartup which creates the MainWindow
            InitializeServices();
            
            // Now call base.OnStartup which will create MainWindow using StartupUri
            base.OnStartup(e);
        }

        private void InitializeServices()
        {
            var builder = Host.CreateDefaultBuilder();

            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddEnvironmentVariables();
            });

            builder.ConfigureServices((context, services) =>
            {
                var configuration = context.Configuration;

                // Database
                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

                // Services
                services.AddSingleton<DatabaseService>();
                services.AddSingleton<ChatGPTJapaneseService>();
                services.AddSingleton<SRSCalculationService>();
                services.AddSingleton<JLPTService>();
                services.AddSingleton<KanjiRadicalService>();
                services.AddSingleton<ImportExportService>();
                services.AddSingleton<PerformanceMonitoringService>();

                // ViewModels
                services.AddTransient<MainViewModel>();
                services.AddTransient<DashboardViewModel>();
                services.AddTransient<KanjiViewModel>();
                services.AddTransient<VocabularyViewModel>();
                services.AddTransient<GrammarViewModel>();
                services.AddTransient<PracticeViewModel>();
                services.AddTransient<JLPTProgressViewModel>();
            });

            _host = builder.Build();

            // Initialize database with error handling
            try
            {
                using (var scope = _host.Services.CreateScope())
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<App>>();
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    
                    logger.LogInformation("Starting database initialization...");
                    dbContext.Database.EnsureCreated();
                    logger.LogInformation("Database initialization completed successfully.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database initialization failed: {ex.Message}", "Database Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _host?.Dispose();
            base.OnExit(e);
        }

        public static T GetService<T>()
            where T : class
        {
            var app = Current as App;
            if (app?._host == null)
            {
                throw new InvalidOperationException("Application host is not initialized");
            }

            var service = app._host.Services.GetService(typeof(T)) as T;
            if (service == null)
            {
                throw new InvalidOperationException($"Service {typeof(T)} not found");
            }

            return service;
        }
    }
}
