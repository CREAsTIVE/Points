using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace Points.Windows.SignalViewer {
	/// <summary>
	/// Interaction logic for SignalViewWindow.xaml
	/// </summary>
	public partial class SignalViewerWindow : Window {
		public SignalViewerWindow(IServiceProvider serviceProvider) {
			InitializeComponent();
			var dataContext = serviceProvider.GetRequiredService<SignalViewerViewModel>();
			dataContext.parentWindow = this;
			DataContext = dataContext;
		}
	}
}
