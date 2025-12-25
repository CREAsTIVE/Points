using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using Points.Services.Database;
using Points.Utils;
using Points.Windows;
using Points.Windows.SignalCreation;
using Points.Windows.SignalSelection;
using Points.Windows.SignalView;
using System.Runtime.InteropServices;
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
			MessageBox.Show(errorMessage, "Critical error!", MessageBoxButton.OK, MessageBoxImage.Error);
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

			// Windows:
			services.RegisterWindow<SignalSelectionWindow, SignalSelectionViewModel>();
			services.RegisterWindow<SignalCreationWindow, SignalCreationViewModel>();
			services.RegisterWindow<SignalViewWindow, SignalViewViewModel>();

			#endregion
			serviceProvider = services.BuildServiceProvider();

			try {
				// Maybe async?
				serviceProvider.GetService<IDbContextFactory<SignalDbContext>>()!.CreateDbContext().Database.EnsureCreated();
			} catch (Exception ex) {
				CrashApplication("Database initialization failed", ex);
			}

			serviceProvider.GetRequiredService<SignalSelectionWindow>().Show();
		}

		

		protected override async void OnExit(ExitEventArgs e) {
			if (serviceProvider is not null)
				await serviceProvider.DisposeAsync();

			base.OnExit(e);
		}
	}

}
