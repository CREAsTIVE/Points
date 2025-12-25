using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Points.Services.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
			var dataContext = serviceProvider.GetRequiredService<SignalViewViewModel>(); ;
			DataContext = dataContext;
		}

		// Binding not working, so we do it manually
		private void XAxis_PropertyChanged(object? sender, PropertyChangedEventArgs e) { 
			if (e.PropertyName is nameof(Axis.MinLimit) or nameof(Axis.MaxLimit)) { 
				if (DataContext is SignalViewViewModel vm) {
					vm.VisibleMin = xAxis.MinLimit ?? 0;
					vm.VisibleMax = xAxis.MaxLimit ?? 100;
				}
			}
		}
	}
}
