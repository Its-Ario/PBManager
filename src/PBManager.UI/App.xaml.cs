using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using SkiaSharp;
using PBManager.MVVM.View;
using PBManager.MVVM.ViewModel;
using PBManager.Infrastructure.Data;
using PBManager.Application.Interfaces;
using PBManager.Application.Services;
using PBManager.Core.Interfaces;
using PBManager.Infrastructure.Repositories;
using PBManager.Infrastructure.Parsers;
using PBManager.Infrastructure.Services;
using System.IO;
using PBManager.Infrastructure.Services.Parsers;

namespace PBManager
{
    public partial class App : System.Windows.Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        public static string DatabasePath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "PBManager",
        "data.db");

        protected override async void OnStartup(StartupEventArgs e)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(DatabasePath));
            await DatabasePorter.HandlePendingImportOnStartupAsync(DatabasePath);

            base.OnStartup(e);

            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            using (var scope = ServiceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                dbContext.Database.EnsureCreated();
                dbContext.Database.Migrate();
            }

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            LiveCharts.Configure(config =>
                config.AddDarkTheme()
                      .HasGlobalSKTypeface(SKFontManager.Default.MatchCharacter('پ'))
                      .UseRightToLeftSettings()
            );
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DatabaseContext>(options =>
                options.UseSqlite($"Data Source={DatabasePath}"));

            services.AddMemoryCache();

            services.AddScoped<IClassRepository, ClassRepository>();
            services.AddScoped<IStudentRepository, StudentRepository>();
            services.AddScoped<ISubjectRepository, SubjectRepository>();
            services.AddScoped<IStudyRecordRepository, StudyRecordRepository>();

            services.AddScoped<IClassService, ClassService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<ISubjectService, SubjectService>();
            services.AddScoped<IStudyRecordService, StudyRecordService>();

            services.AddTransient<IDatabasePorter, DatabasePorter>();
            services.AddTransient<IDatabaseManagementService, DatabaseManagementService>();

            services.AddTransient<HomeViewModel>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<StudentDetailViewModel>();
            services.AddTransient<StudyManagementViewModel>();
            services.AddTransient<AddStudyRecordViewModel>();

            services.AddTransient<MainWindow>();
            services.AddTransient<AddStudyRecordView>();
            services.AddTransient<StudyHistoryView>();

            services.AddTransient<XlsxStudentParser>();
            services.AddTransient<CsvStudentParser>();
        }
    }
}
