using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView.WPF;
using Microsoft.EntityFrameworkCore;
using Points.Models;
using Points.Services.Database;
using Points.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Points.Windows.SignalView; 
public partial class SignalViewViewModel(IDbContextFactory<SignalDbContext> dbFactory) : ObservableObject {
	[ObservableProperty]
	SignalModel? selectedSignal;

	[ObservableProperty]
	ObservableLinkedList<List<float>> loadedChunks = new();

	[ObservableProperty]
	private double visibleMin = 0;
	bool minUpdating = false;
	int loadedChunkMin = 0;

	[ObservableProperty]
	private double visibleMax = 100;
	bool maxUpdating = false;
	int loadedChunkMax = 1;

	partial void OnVisibleMinChanged(double value) {
		if (selectedSignal is null) return;
		if (minUpdating) return;
		minUpdating = true;
		_ = OnVisibleMinChangedAsync(selectedSignal, value);
	}

	private async Task OnVisibleMinChangedAsync(SignalModel signal, double value) {
		var newMin = (int)Math.Min(value / signal.ChunkSize, 0);
		if (newMin < loadedChunkMin) {
			for (int i = newMin; i < loadedChunkMin; i++) {
				using (var db = dbFactory.CreateDbContext()) {
					LoadedChunks.AddFirst(await signal.GetChunk(db, i));
				}
			}
		} else if (newMin > loadedChunkMin) {
			for (int i = loadedChunkMin; i < newMin; i++) {
				LoadedChunks.RemoveFirst();
			}
		}

		loadedChunkMin = newMin;
		minUpdating = false;
	}

	partial void OnVisibleMaxChanged(double value) {
		if (selectedSignal is null) return;
		if (maxUpdating) return;
		maxUpdating = true;
		_ = OnVisibleMaxChangedAsync(selectedSignal, value);
	}

	private async Task OnVisibleMaxChangedAsync(SignalModel signal, double value) {
		var newMax = (int)Math.Max(value / signal.ChunkSize, signal.ChunkAmount - 1);
		if (newMax > loadedChunkMax) {
			for (int i = loadedChunkMax + 1; i <= newMax; i++) {
				using (var db = dbFactory.CreateDbContext()) {
					LoadedChunks.AddLast(await signal.GetChunk(db, i));
				}
			}
		} else if (newMax < loadedChunkMax) {
			for (int i = loadedChunkMax; i > newMax; i--) {
				LoadedChunks.RemoveLast();
			}
		}

		loadedChunkMax = newMax;
		maxUpdating = false;
	}

	partial void OnLoadedChunksChanged(ObservableLinkedList<List<float>> value) {
		OnPropertyChanged(nameof(ChunkYValues));
	}

	public IEnumerable<ObservablePoint> ChunkYValues => LoadedChunks.SelectMany(v => v).Select((v, i) => new ObservablePoint(i, v));


	partial void OnSelectedSignalChanged(SignalModel? value) {
		if (value is null) return;
		_ = LoadSignal(value);
		
	}
	public async Task LoadSignal(SignalModel selectedSignal) {
		using (var db = await dbFactory.CreateDbContextAsync()) {
			LoadedChunks = new(await selectedSignal.GetChunks(db, 0, 1));
		}
	}
}
