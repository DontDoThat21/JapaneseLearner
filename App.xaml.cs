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
            var builder = Host.CreateDefaultBuilder(e.Args);

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

            // Initialize database
            using (var scope = _host.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.EnsureCreated();
            }

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _host?.Dispose();
            base.OnExit(e);
        }

        public static T GetService<T>()
            where T : class
        {
            return ((App)Current)._host?.Services.GetService(typeof(T)) as T
                ?? throw new InvalidOperationException($"Service {typeof(T)} not found");
        }
    }
}
