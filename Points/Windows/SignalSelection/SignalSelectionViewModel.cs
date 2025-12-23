using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Points.Models;
using Points.Services.Database;
using Points.Windows.SignalCreation;
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
	public SignalSelectionViewModel(IDbContextFactory<SignalDbContext> dbFactory, Func<SignalCreationWindow> signalCreationWindowFactory) {
		this.dbFactory = dbFactory;
		this.signalCreationWindowFactory = signalCreationWindowFactory;
		_ = Load();
	}

	[ObservableProperty]
	ObservableCollection<SignalModel> signals = new();

	[ObservableProperty]
	SignalModel? selectedSignal = null;

	public async Task Load() {
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

	}
}
