using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using SkiaSharp;
using PBManager.Infrastructure.Data;
using PBManager.Application.Interfaces;
using PBManager.Application.Services;
using PBManager.Core.Interfaces;
using PBManager.Infrastructure.Repositories;
using PBManager.Infrastructure.Parsers;
using PBManager.Infrastructure.Services;
using System.IO;
using PBManager.Infrastructure.Services.Parsers;
using PBManager.Infrastructure.Exporters;
using PBManager.UI.Services;
using PBManager.UI.MVVM.View;
using PBManager.UI.MVVM.ViewModel;
using PBManager.Infrastructure;

namespace PBManager
{
    public partial class App : System.Windows.Application
    {
        public static IServiceProvider? ServiceProvider { get; private set; }

        public static string DatabasePath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "PBManager",
        "data.db");

        protected override async void OnStartup(StartupEventArgs e)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(DatabasePath) ?? string.Empty);

            base.OnStartup(e);

            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
            bool firstStartup;

            using (var scope = ServiceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                firstStartup = !await dbContext.Database.CanConnectAsync();
                await dbContext.Database.MigrateAsync();
            }

            if (firstStartup)
            {
                var startupWindow = ServiceProvider.GetRequiredService<StartupView>();
                startupWindow.Show();
            }
            else
            {
                var loginWindow = ServiceProvider.GetRequiredService<LoginView>();
                loginWindow.Show();
            }

            LiveCharts.Configure(config =>
                config.AddDarkTheme()
                      .HasGlobalSKTypeface(SKFontManager.Default.MatchCharacter('پ'))
                      .UseRightToLeftSettings()
            );
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DatabaseContext>(options =>
                options.UseSqlite($"Data Source={DatabasePath}"));

            services.AddMemoryCache();

            services.AddScoped<IClassRepository, ClassRepository>();
            services.AddScoped<IStudentRepository, StudentRepository>();
            services.AddScoped<ISubjectRepository, SubjectRepository>();
            services.AddScoped<IStudyRecordRepository, StudyRecordRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            services.AddScoped<IGradeRepository, GradeRepository>();
            services.AddScoped<IExamRepository, ExamRepository>();

            services.AddScoped<IClassService, ClassService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<ISubjectService, SubjectService>();
            services.AddScoped<IStudyRecordService, StudyRecordService>();
            services.AddScoped(typeof(IManagementService<>), typeof(ManagementService<>));
            services.AddSingleton<IDialogService, DialogService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IAuditLogService, AuditLogService>();
            services.AddScoped<IGradeService, GradeService>();
            services.AddScoped<IExamService, ExamService>();
            services.AddSingleton<IUserSession, UserSession>();

            services.AddTransient<IDatabasePorter, DatabasePorter>();
            services.AddTransient<IDatabaseManagementService, DatabaseManagementService>();

            services.AddTransient<HomeViewModel>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<StudentDetailViewModel>();
            services.AddTransient<StudyOverviewViewModel>();
            services.AddTransient<StudentManagementViewModel>();
            services.AddTransient<AddStudyRecordViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<HistoryViewModel>();
            services.AddTransient<StudyHistoryViewModel>();
            services.AddTransient<GradeOverviewViewModel>();
            services.AddTransient<AddGradeRecordViewModel>();
            services.AddTransient<ExamManagementViewModel>();
            services.AddTransient<AddExamViewModel>();
            services.AddTransient<ExamOverviewViewModel>();
            services.AddTransient<StartupViewModel>();

            services.AddTransient<MainWindow>();
            services.AddTransient<AddStudyRecordView>();
            services.AddTransient<StudyOverviewView>();
            services.AddTransient<StudyHistoryView>();
            services.AddTransient<AddGradeRecordView>();
            services.AddTransient<LoginView>();
            services.AddTransient<AddExamView>();
            services.AddTransient<StartupView>();

            services.AddTransient<XlsxStudentParser>();
            services.AddTransient<CsvStudentParser>();
            services.AddTransient<XlsxStudentExporter>();
        }
    }
}
