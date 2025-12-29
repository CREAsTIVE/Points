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

	[RelayCommand]
	public void MergeSelectedSignal() {
		var window = signalSelectionWindowFactory();
		if (SelectedSignal is null) return;
		var signal1 = SelectedSignal.Signal;

		((SignalSelectionViewModel)window.DataContext).OnSignalSelect = signal2 => {
			async Task MergeSignals() {
				var newSignal = new SignalModel(await SignalMetaEntity.Create(new() {
					Name = $"+({signal1.Name}, {signal2.Name})",
					TimeStep = signal1.TimeStep, // WARN: We assume that timestep is same
				}, dbFactory));

				var points1 = (await signal1.GetChunks(dbFactory, 0, signal1.TotalChunks)).SelectMany(p => p);
				var points2 = (await signal2.GetChunks(dbFactory, 0, signal2.TotalChunks)).SelectMany(p => p);

				await newSignal.SetPoints(dbFactory, points1.ZipNullable(points2).Select(pair => pair.a + pair.b).ToAsyncEnumerable()); // TODO: Hardcoded value, settings for modification

				await Application.Current.Dispatcher.InvokeAsync(() => AddSignal(newSignal));
			}

			_ = Task.Run(MergeSignals);
		};

		window.Show();
	}

	[RelayCommand]
	public async Task CalculateFFT(SignalModel model) {
		await Task.Run(async () => {
			var amplitudes = Utils.FFTUtils.ComputeFFTWithFrequencies(
				(await model.GetChunks(dbFactory, 0, model.TotalChunks)).SelectMany(e => e).ToList(),
				1000
			).Select(v => v.Amplitude).ToList();

			var newSignal = new SignalModel(await SignalMetaEntity.Create(new() {
				Name = $"БПФ {model.Name}",
				TimeStep = 1
			}, dbFactory));

			await newSignal.SetPoints(dbFactory, amplitudes.ToAsyncEnumerable());

			await Application.Current.Dispatcher.InvokeAsync(() => AddSignal(newSignal));
		});
	}
}
