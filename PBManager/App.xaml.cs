using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using PBManager.Data;
using System;
using System.Windows;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using PBManager.Services;
using SkiaSharp;
using PBManager.MVVM.View;
using PBManager.MVVM.ViewModel;

namespace PBManager
{
    public partial class App : Application
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

            services.AddTransient<StudentService>();
            services.AddTransient<StudyRecordService>();
            services.AddTransient<SubjectService>();
            services.AddTransient<ClassService>();

            services.AddTransient<HomeViewModel>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<StudyManagementViewModel>();

            services.AddTransient<MainWindow>();
            services.AddTransient<AddStudyRecordView>();
            services.AddTransient<StudyHistoryView>();
        }
    }
}
