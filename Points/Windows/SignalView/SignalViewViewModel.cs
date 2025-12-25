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
using System.Windows;

namespace Points.Windows.SignalView; 
public partial class SignalViewViewModel(IDbContextFactory<SignalDbContext> dbFactory, Func<SignalSelectionWindow> signalSelectionWindowFactory) : ObservableObject {
	SignalModel? signal;

	[ObservableProperty]
	ChunkedList<float>? loadedChunks;

	#region leftBorder
	private double leftBorder = 0;
	bool leftBorderLock = false;
	int leftBorderCurrentChunk = 0;

	private async Task UpdateLeftBorder(double value) {
		if (leftBorderLock) return;
		leftBorderLock = true;

		if (signal is null) return;
		if (LoadedChunks is null) return;

		bool updateChart = false;

		var newMin = (int) Math.Clamp(value / signal.ChunkSize + 0.001, 0, signal.ChunkAmount - 1); // + 0.001 for float point error fix

		if (newMin < leftBorderCurrentChunk) {
			for (int i = newMin; i < leftBorderCurrentChunk; i++) {
				LoadedChunks.AddFirstChunk(await signal.GetChunk(dbFactory, i));
			}
			updateChart = true;
		} 
		else if (newMin > leftBorderCurrentChunk) {
			for (int i = leftBorderCurrentChunk; i < newMin; i++) {
				LoadedChunks.RemoveFirstChunk();
			}
			updateChart = true;
		}

		if (updateChart) {
			PlotControl.Refresh();
			leftBorderCurrentChunk = newMin;
			if (currentPlot is not null) {
				currentPlot!.Data.XOffset = newMin * signal.ChunkSize * signal.TimeStep;
			}
		}

		leftBorderLock = false; // better if we do another update, when second update recieved and cancel prev
	}
	#endregion

	#region rightBorder
	private double rightBorder = 0;
	bool rightBorderLock = false;
	int rightBorderCurrentChunk = 0;

	private async Task UpdateRightBorder(double value) {
		if (rightBorderLock) return;
		rightBorderLock = true;

		if (signal is null) return;
		if (LoadedChunks is null) return;

		bool updateChart = false;

		var newMax = (int) Math.Clamp(value / signal.ChunkSize + 1 + 0.001, 0, signal.ChunkAmount - 1);

		if (newMax > rightBorderCurrentChunk) {
			for (int i = rightBorderCurrentChunk + 1; i <= newMax; i++) {
				LoadedChunks.AddLastChunk(await signal.GetChunk(dbFactory, i));
			}
			updateChart = true;
		} else if (newMax < rightBorderCurrentChunk) {
			for (int i = rightBorderCurrentChunk; i > newMax; i--) {
				LoadedChunks.RemoveLastChunk();
			}
			updateChart = true;
		}

		if (updateChart) {
			PlotControl.Refresh();
			rightBorderCurrentChunk = newMax;
		}

		rightBorderLock = false;
	}
	#endregion

	public WpfPlot PlotControl { get; } = new();
	Signal? currentPlot;

	public void SetSignal(SignalModel? value) {
		if (value is null) return;
		_ = LoadSignal(value);
	}
	public async Task LoadSignal(SignalModel signal) {
		this.signal = signal;

		PlotControl.Plot.Clear();

		using (var db = await dbFactory.CreateDbContextAsync()) {
			LoadedChunks = new(signal.ChunkSize);
			LoadedChunks.AddFirstChunk(await signal.GetChunk(db, 0));
		}

		currentPlot = PlotControl.Plot.Add.Signal(LoadedChunks);

		PlotControl.Plot.RenderManager.RenderStarting += (obj, e) => {
			_ = UpdateLeftBorder(PlotControl.Plot.Axes.GetLimits().Left / signal.TimeStep);
			_ = UpdateRightBorder(PlotControl.Plot.Axes.GetLimits().Right / signal.TimeStep);
		};

		currentPlot.Data.Period = signal.TimeStep;
	}

	// Bad practice
	public required Window parentWindow;

	[RelayCommand]
	public void OpenOtherSignal() {
		OpenOtherSignalThisWindow();
		parentWindow.Close();
	}

	[RelayCommand]
	public void OpenOtherSignalThisWindow() {
		signalSelectionWindowFactory().Show();
	}
}
