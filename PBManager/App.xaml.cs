using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.EntityFrameworkCore;
using PBManager.Data;
using SkiaSharp;
using System.Configuration;
using System.Data;
using System.Windows;

namespace PBManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static DatabaseContext Db { get; private set; }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseSqlite("Data Source=data.db")
            .Options;

            Db = new DatabaseContext(options);

            Db.Database.EnsureCreated();

            LiveCharts.Configure(config =>
            config
                .AddDarkTheme()  
                .HasGlobalSKTypeface(SKFontManager.Default.MatchCharacter('پ'))
                .UseRightToLeftSettings()

            );
        }
    }
}
