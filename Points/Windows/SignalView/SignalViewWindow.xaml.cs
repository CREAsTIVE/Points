using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace Points.Windows.SignalView {
	/// <summary>
	/// Interaction logic for SignalViewWindow.xaml
	/// </summary>
	public partial class SignalViewWindow : Window {
		public SignalViewWindow(IServiceProvider serviceProvider) {
			InitializeComponent();
			var dataContext = serviceProvider.GetRequiredService<SignalViewViewModel>();
			dataContext.parentWindow = this;
			DataContext = dataContext;
		}
	}
}
