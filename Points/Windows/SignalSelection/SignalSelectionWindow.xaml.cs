using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Points.Services.Database;
using Points.Windows.SignalCreation;
using Points.Windows.SignalSelection;
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

namespace Points.Windows {
	/// <summary>
	/// Interaction logic for SignalSelectionWindow.xaml
	/// </summary>
	public partial class SignalSelectionWindow : Window {
		public SignalSelectionWindow(IServiceProvider serviceProvider) {
			InitializeComponent();

			var dataContext = serviceProvider.GetRequiredService<SignalSelectionViewModel>(); ;
			dataContext.window = this;
			DataContext = dataContext;
		}
    }
}
