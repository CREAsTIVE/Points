using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Points.Models;
using Points.Services.Database;
using Points.Windows.SignalCreation;
using Points.Windows.SignalView;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;

namespace Points.Windows.SignalSelection; 
public partial class SignalSelectionViewModel : ObservableObject {
	IDbContextFactory<SignalDbContext> dbFactory;
	Func<SignalCreationWindow> signalCreationWindowFactory;
	Func<SignalViewWindow> signalViewWindowFactory;
	public SignalSelectionViewModel(IDbContextFactory<SignalDbContext> dbFactory, Func<SignalCreationWindow> signalCreationWindowFactory, Func<SignalViewWindow> signalViewWindowFactory) {
		this.dbFactory = dbFactory;
		this.signalCreationWindowFactory = signalCreationWindowFactory;
		this.signalViewWindowFactory = signalViewWindowFactory;
		_ = LoadSignals();
	}

	[ObservableProperty]
	ObservableCollection<SignalModel> signals = new();

	[ObservableProperty]
	SignalModel? selectedSignal = null;

	public async Task LoadSignals() {
		using (var db = dbFactory.CreateDbContext()) {
			Signals = new((await db.SignalsMeta.Select(meta => meta).ToListAsync()).Select(entity => new SignalModel(entity)));
		}
	}

	public SignalSelectionWindow? parentWindow;

	[RelayCommand]
	void OpenCreateSignalWindow() {
		var window = signalCreationWindowFactory();
		window.OnCreated += () => parentWindow?.Close();
		window.ShowDialog();
	}

	[RelayCommand]
	void SelectSignal(SignalModel signal) {
		var window = signalViewWindowFactory();
		parentWindow?.Close();

		((SignalViewViewModel)window.DataContext).SetSignal(signal);
		window.Show();
	}

	[RelayCommand]
	async Task DeleteSignal(SignalModel signal) {
		using (var db = await dbFactory.CreateDbContextAsync()) {
			db.SignalsMeta.Remove(signal.entity);
			await db.SaveChangesAsync();
			await LoadSignals();
		}
	}
}
