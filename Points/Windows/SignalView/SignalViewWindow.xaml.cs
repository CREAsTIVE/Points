using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Points.Services.Database;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Points.Windows.SignalView {
	/// <summary>
	/// Interaction logic for SignalViewWindow.xaml
	/// </summary>
	public partial class SignalViewWindow : Window {
		public SignalViewWindow(IServiceProvider serviceProvider) {
			InitializeComponent();
			DataContext = serviceProvider.GetRequiredService<SignalViewViewModel>();
		}
	}
}
