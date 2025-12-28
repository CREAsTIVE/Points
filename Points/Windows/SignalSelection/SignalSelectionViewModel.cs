using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Points.Models;
using Points.Services.Database;
using Points.Windows.SignalCreation;
using Points.Windows.SignalViewer;
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
	Func<SignalViewerWindow> signalViewWindowFactory;
	public SignalSelectionViewModel(IDbContextFactory<SignalDbContext> dbFactory, Func<SignalCreationWindow> signalCreationWindowFactory, Func<SignalViewerWindow> signalViewWindowFactory) {
		this.dbFactory = dbFactory;
		this.signalCreationWindowFactory = signalCreationWindowFactory;
		this.signalViewWindowFactory = signalViewWindowFactory;

		OnSignalSelect = (signal) => {
			var window = signalViewWindowFactory();
			((SignalViewerViewModel)window.DataContext).AddSignal(signal);
			window.Show();
		};

		_ = LoadSignals();
	}

	[ObservableProperty]
	ObservableCollection<SignalModel> signals = new();

	[ObservableProperty]
	SignalModel? selectedSignal = null;

	[ObservableProperty]
	Action<SignalModel> onSignalSelect = delegate { };

	public async Task LoadSignals() {
		using (var db = dbFactory.CreateDbContext()) {
			Signals = new((await db.SignalsMeta.Select(meta => meta).ToListAsync()).Select(entity => new SignalModel(entity)));
		}
	}

	public SignalSelectionWindow? parentWindow;

	[RelayCommand]
	void OpenCreateSignalWindow() {
		var window = signalCreationWindowFactory();
		window.OnCreated += (signal) => SelectSignal(signal);
		window.ShowDialog();
	}

	[RelayCommand]
	void SelectSignal(SignalModel signal) {
		OnSignalSelect(signal);

		parentWindow?.Close();
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
