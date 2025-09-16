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

namespace PBManager
{
    public partial class App : System.Windows.Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();

            ConfigureServices(services);

            ServiceProvider = services.BuildServiceProvider();

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
                options.UseSqlite("Data Source=data.db"));

            services.AddMemoryCache();

            services.AddScoped<IClassRepository, ClassRepository>();
            services.AddScoped<IStudentRepository, StudentRepository>();
            services.AddScoped<ISubjectRepository, SubjectRepository>();
            services.AddScoped<IStudyRecordRepository, StudyRecordRepository>();

            services.AddScoped<IClassService, ClassService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<ISubjectService, SubjectService>();
            services.AddScoped<IStudyRecordService, StudyRecordService>();

            services.AddTransient<HomeViewModel>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<StudentDetailViewModel>();
            services.AddTransient<StudyManagementViewModel>();
            services.AddTransient<AddStudyRecordViewModel>();

            services.AddTransient<MainWindow>();
            services.AddTransient<AddStudyRecordView>();
            services.AddTransient<StudyHistoryView>();
        }
    }
}
