using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Points.Models;
using Points.Services.Database;
using Points.Utils;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.WPF;

namespace Points.Windows.SignalView; 
public partial class SignalViewViewModel(IDbContextFactory<SignalDbContext> dbFactory) : ObservableObject {
	[ObservableProperty]
	SignalModel? selectedSignal;

	[ObservableProperty]
	ObservableChunkedList<float>? loadedChunks;

	private double visibleMin = 0;
	bool minUpdating = false;
	int loadedChunkMin = 0;

	private double visibleMax = 100;
	bool maxUpdating = false;
	int loadedChunkMax = 1;

	public WpfPlot PlotControl { get; } = new();
	Signal? currentPlot;

	void OnVisibleMinChanged(double value) {
		if (SelectedSignal is null) return;
		if (minUpdating) return;
		minUpdating = true;
		_ = OnVisibleMinChangedAsync(SelectedSignal, value);
	}

	private async Task OnVisibleMinChangedAsync(SignalModel signal, double value) {
		if (LoadedChunks is null) return;
		var newMin = (int)Math.Max(value / signal.ChunkSize, 0);
		if (newMin < loadedChunkMin) {
			for (int i = newMin; i < loadedChunkMin; i++) {
				using (var db = dbFactory.CreateDbContext()) {
					LoadedChunks.AddFrontChunk(await signal.GetChunk(db, i));
				}
			}
			PlotControl.Refresh();
		} else if (newMin > loadedChunkMin) {
			for (int i = loadedChunkMin; i < newMin; i++) {
				LoadedChunks.RemoveFrontChunk();
			}
			PlotControl.Refresh();
		}

		loadedChunkMin = newMin;
		minUpdating = false;
	}

	void OnVisibleMaxChanged(double value) {
		if (SelectedSignal is null) return;
		if (maxUpdating) return;
		maxUpdating = true;
		_ = OnVisibleMaxChangedAsync(SelectedSignal, value);
	}

	private async Task OnVisibleMaxChangedAsync(SignalModel signal, double value) {
		if (LoadedChunks is null) return;

		var newMax = (int)Math.Min(value / signal.ChunkSize + 1, signal.ChunkAmount - 1);
		if (newMax > loadedChunkMax) {
			for (int i = loadedChunkMax + 1; i <= newMax; i++) {
				using (var db = dbFactory.CreateDbContext()) {
					LoadedChunks.AddLastChunk(await signal.GetChunk(db, i));
				}
			}
			PlotControl.Refresh();
		} else if (newMax < loadedChunkMax) {
			for (int i = loadedChunkMax; i > newMax; i--) {
				LoadedChunks.RemoveLastChunk();
			}
			PlotControl.Refresh();
		} 

		loadedChunkMax = newMax;
		if (currentPlot is not null) {
			currentPlot.Data.XOffset = loadedChunkMin * signal.ChunkSize;
		}
		maxUpdating = false;
	}


	partial void OnSelectedSignalChanged(SignalModel? value) {
		if (value is null) return;
		_ = LoadSignal(value);
		
	}
	public async Task LoadSignal(SignalModel selectedSignal) {
		PlotControl.Plot.Clear();

		using (var db = await dbFactory.CreateDbContextAsync()) {
			LoadedChunks = new(selectedSignal.ChunkSize);
			LoadedChunks.AddFrontChunk(await selectedSignal.GetChunk(db, 0));
		}

		currentPlot = PlotControl.Plot.Add.Signal(LoadedChunks);

		PlotControl.Plot.RenderManager.RenderStarting += (obj, e) => {
			OnVisibleMinChanged(PlotControl.Plot.Axes.GetLimits().Left);
			OnVisibleMaxChanged(PlotControl.Plot.Axes.GetLimits().Right);
		};
	}
}
