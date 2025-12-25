using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Points.Models;
using Points.Services.Database;
using Points.Windows.SignalCreation.Presets;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Points.Windows.SignalCreation {
	/// <summary>
	/// Interaction logic for SignalCreationWindow.xaml
	/// </summary>
	public partial class SignalCreationWindow : Window {
		public event Action OnCreated = delegate { };
		public event Action OnCanceled = delegate { };
		IDbContextFactory<SignalDbContext> dbFactory;

		public bool created = false;
		
		public SignalCreationWindow(IServiceProvider service, IDbContextFactory<SignalDbContext> dbFactory) {
			InitializeComponent();

			var dataContext = service.GetRequiredService<SignalCreationViewModel>();
			dataContext.baseWindow = this;
			DataContext = dataContext;

			this.dbFactory = dbFactory;
		}
		public void Created() {
			created = true;
			this.Close();
			OnCreated();
		}
		private void OnClosed(object sender, EventArgs e) {
			if (!created) OnCanceled();
		}
	}
}
