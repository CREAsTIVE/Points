using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Points.Models;
using Points.Services.Database;
using Points.Utils;
using Points.Windows.SignalSelection;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.WPF;
using System.Collections.ObjectModel;
using System.Security.Policy;
using System.Windows;

namespace Points.Windows.SignalViewer; 
public partial class SignalViewerViewModel : ObservableObject {
	public WpfPlot PlotControl { get; } = new();

	[ObservableProperty]
	ObservableCollection<SignalViewViewModel> signals = new();

	[ObservableProperty]
	SignalViewViewModel? selectedSignal;

	// Bad practice
	public required Window parentWindow;
	private readonly Func<SignalSelectionWindow> signalSelectionWindowFactory;
	private readonly IDbContextFactory<SignalDbContext> dbFactory;

	/// <summary>
	/// For DI construction only
	/// </summary>
	public SignalViewerViewModel(IDbContextFactory<SignalDbContext> dbFactory, Func<SignalSelectionWindow> signalSelectionWindowFactory) {
		this.signalSelectionWindowFactory = signalSelectionWindowFactory;
		this.dbFactory = dbFactory;

		PlotControl.Plot.RenderManager.RenderStarting += (obj, e) => {
			foreach (var signal in signals) {
				signal.OnChartUpdate();
			}
		};
	}

	public void AddSignal(SignalModel signal) {
		Signals.Add(new(signal, PlotControl, dbFactory));
		if (SelectedSignal is null) {
			SelectedSignal = Signals[0];
		}
	}

	[RelayCommand]
	public void RemoveSignalView(SignalViewViewModel signalView) {
		signalView.RemovePlot();
		Signals.Remove(signalView);

		if (SelectedSignal == signalView) {
			SelectedSignal = null;
		}
	}

	[RelayCommand]
	public void OpenOtherSignalNewWindow() {
		signalSelectionWindowFactory().Show();
	}

	[RelayCommand]
	public void OpenOtherSignalThisWindow() {
		var window = signalSelectionWindowFactory();
		((SignalSelectionViewModel)window.DataContext).OnSignalSelect = (signal) => {
			AddSignal(signal);
		};
		window.Show();
	}
}
