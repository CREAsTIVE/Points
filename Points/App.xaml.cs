using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using Points.Services.Database;
using System;
using System.Configuration;
using System.Data;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;


namespace Points
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
		ServiceProvider? serviceProvider;
		Logger globalLogger = LogManager.GetLogger("App");

		public void CrashApplication(string errorMessage, Exception details) {
			globalLogger.Error(details);
			MessageBox.Show(errorMessage, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
			Application.Current.Shutdown(); // should call OnExit();
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool AllocConsole();

		protected override void OnStartup(StartupEventArgs e) {
			base.OnStartup(e);

#if DEBUG
			AllocConsole();
			globalLogger.Warn("Application builded in debug mode...");
#endif

			var services = new ServiceCollection();
			#region Services
			services.AddLogging(builder => {
				builder.ClearProviders();
				builder.AddNLog();
			});

			services.AddDbContextFactory<SignalDbContext>(options =>
				options.UseSqlite("Data Source=signals.db")
			);

			
			#endregion
			serviceProvider = services.BuildServiceProvider();

			try {
				// Maybe async?
				serviceProvider.GetService<IDbContextFactory<SignalDbContext>>()!.CreateDbContext().Database.Migrate();
			} catch (Exception ex) {
				CrashApplication("Database initialization failed", ex);
			}


		}

		protected override async void OnExit(ExitEventArgs e) {
			if (serviceProvider is not null)
				await serviceProvider.DisposeAsync();

			base.OnExit(e);
		}
	}

}
