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
	bool leftBorderLock = false;
	int leftBorderCurrentChunk = 0;

	private async Task UpdateLeftBorder(double value) {
		if (leftBorderLock) return;
		leftBorderLock = true;

		if (signal is null) return;
		if (LoadedChunks is null) return;

		// + 0.001 for float point error fix
		int GetNewMin(double value) => (int)Math.Clamp(value / signal.ChunkSize + 0.001, 0, signal.TotalChunks - 1);
		int newMin = GetNewMin(value); 

		while (newMin < leftBorderCurrentChunk) { // may continue over one frame, need all other checks after
			var chunk = await Task.Run(() => signal.GetChunk(dbFactory, leftBorderCurrentChunk - 1));

			// Add chunk before
			LoadedChunks.AddFirstChunk(chunk);
			leftBorderCurrentChunk--;

			_ = Task.Run(() => {
				PlotControl.Refresh();
				if (currentPlot is not null) {
					currentPlot!.Data.XOffset = newMin * signal.ChunkSize * signal.TimeStep;
				}
			});

			newMin = GetNewMin(value); // Recalculate newMin after Task.Run
		}

		if (newMin > leftBorderCurrentChunk) { // done in one frame

			// removed all chunks before newMin
			for (int i = leftBorderCurrentChunk; i < newMin; i++) {
				LoadedChunks.RemoveFirstChunk();
			}
			leftBorderCurrentChunk = newMin;

			_ = Task.Run(() => {
				PlotControl.Refresh();
				if (currentPlot is not null) {
					currentPlot!.Data.XOffset = newMin * signal.ChunkSize * signal.TimeStep;
				}
			});
		}

		leftBorderLock = false;

		
	}
	#endregion

	#region rightBorder
	bool rightBorderLock = false;
	int rightBorderCurrentChunk = 0;

	// Almost same as UpdateLeftBorder
	private async Task UpdateRightBorder(double value) {
		if (rightBorderLock) return;
		rightBorderLock = true;

		if (signal is null) return;
		if (LoadedChunks is null) return;

		int getNewMax(double value) => (int)Math.Clamp(value / signal.ChunkSize + 1 + 0.001, 0, signal.TotalChunks - 1);
		int newMax = getNewMax(value);

		while (newMax > rightBorderCurrentChunk) { // TODO: cut rightBorderCurrentChunk if leftBorderCurrentChunk passed it (and vice versa)
			var chunk = await Task.Run(() => signal.GetChunk(dbFactory, rightBorderCurrentChunk + 1));

			LoadedChunks.AddLastChunk(chunk);
			rightBorderCurrentChunk++;

			_ = Task.Run(PlotControl.Refresh);

			newMax = getNewMax(value);
		}

		if (newMax < rightBorderCurrentChunk) {
			for (int i = rightBorderCurrentChunk; i > newMax; i--) {
				LoadedChunks.RemoveLastChunk();
			}

			rightBorderCurrentChunk = newMax;

			_ = Task.Run(PlotControl.Refresh);
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
