using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using NLog.Config;
using Points.Models;
using Points.Services.Database;
using Points.Utils;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Points.Windows.SignalViewer; 

/// <summary>
/// Holds loaded chunks and displays them
/// </summary>
public partial class SignalViewViewModel : ObservableObject {
	[ObservableProperty]
	SignalModel signal;

	[ObservableProperty]
	ChunkedList<float> loadedChunks;
	Signal plot;

	private readonly WpfPlot plotControl;
	private readonly IDbContextFactory<SignalDbContext> dbFactory;

	/// <summary>
	/// For manuall construction only
	/// </summary>
	public SignalViewViewModel(
		SignalModel signal,
		WpfPlot plotControl,
		IDbContextFactory<SignalDbContext> dbFactory
	) {
		this.plotControl = plotControl;
		this.dbFactory = dbFactory;
		this.signal = signal;
		this.loadedChunks = new(signal.ChunkSize);

		plot = plotControl.Plot.Add.Signal(loadedChunks);
		plot.Data.Period = signal.TimeStep;
	}

	public void RemovePlot() {
		plotControl.Plot.Remove(plot);
		plotControl.Refresh();
	}

	public void OnChartUpdate() {
		_ = UpdateLeftBorder(); 
		_ = UpdateRightBorder();
	}


	#region leftBorder
	bool leftBorderLock = false;
	int leftBorderLoadedChunk = 0;
	int leftBorderScreenChunk = 0;

	// TODO: Move every modification on its own thread and fix race condition with that (commit c64e3836f038cac4ee6e9dfe6b7c83f61f1b550f)
	private async Task UpdateLeftBorder() {
		if (leftBorderLock) return;
		leftBorderLock = true;

		// + 0.001 for float point error fix
		int GetNewLeftBorder() {
			var border = plotControl.Plot.Axes.GetLimits().Left / Signal.TimeStep;
			return (int)Math.Clamp(border / Signal.ChunkSize + 0.001, 0, Signal.TotalChunks - 1);
		}

		leftBorderScreenChunk = GetNewLeftBorder();

		while (leftBorderScreenChunk < leftBorderLoadedChunk) { // may continue over one frame, need all other checks after
			// logger.LogDebug($"Left border far away, fetching chunk {leftBorderCurrentChunk - 1}");
			var chunk = await Task.Run(() => Signal.GetChunk(dbFactory, leftBorderLoadedChunk - 1));

			// Add chunk before
			// TODO: a lot of computation for 1000+ chunks there cause AddFirst does a lot of copies.
			// See ChunkedList for more info
			LoadedChunks.AddFirstChunk(chunk);
			leftBorderLoadedChunk--;
			plot.Data.XOffset = leftBorderLoadedChunk * Signal.ChunkSize * Signal.TimeStep;

			_ = Task.Run(() => {
				plotControl.Refresh();
			});

			leftBorderScreenChunk = GetNewLeftBorder(); // Recalculate newMin after Task.Run
		}

		if (leftBorderScreenChunk > leftBorderLoadedChunk) { // done in one frame
			leftBorderScreenChunk = Math.Min(leftBorderScreenChunk, rightBorderScreenChunk);

			// EDGE-CASE: when left border goes over right border and vice versa
			// it should delete only existing chunks
			int amount = Math.Min(leftBorderScreenChunk - leftBorderLoadedChunk, LoadedChunks.ChunkCount);
			await Task.Run(() => {
				LoadedChunks.RemoveFirstNChunks(amount);
				
				leftBorderLoadedChunk = leftBorderScreenChunk;

				// EDGE-CASE: and move other border with itself
				rightBorderLoadedChunk = Math.Max(rightBorderLoadedChunk, leftBorderLoadedChunk);

				plot.Data.XOffset = leftBorderLoadedChunk * Signal.ChunkSize * Signal.TimeStep;
			}); 
			_ = Task.Run(() => {
				plotControl.Refresh();
			});
		}

		leftBorderLock = false;
	}
	#endregion

	#region rightBorder
	bool rightBorderLock = false;
	int rightBorderLoadedChunk = 0;
	int rightBorderScreenChunk = 0;
	
	// Almost same as UpdateLeftBorder
	private async Task UpdateRightBorder() {
		if (rightBorderLock) return;
		rightBorderLock = true;

		int getNewRightBorder() {
			var border = plotControl.Plot.Axes.GetLimits().Right / Signal.TimeStep;
			return (int)Math.Clamp(border / Signal.ChunkSize + 1 + 0.001, 0, Signal.TotalChunks - 1);
		}

		rightBorderScreenChunk = getNewRightBorder();

		while (rightBorderScreenChunk > rightBorderLoadedChunk) { // TODO: cut rightBorderCurrentChunk if leftBorderCurrentChunk passed it (and vice versa)
			// logger.LogDebug($"Right border far away, fetching chunk {leftBorderCurrentChunk}");
			var chunk = await Task.Run(() => Signal.GetChunk(dbFactory, rightBorderLoadedChunk));

			LoadedChunks.AddLastChunk(chunk);
			rightBorderLoadedChunk++;

			_ = Task.Run(plotControl.Refresh);

			rightBorderScreenChunk = getNewRightBorder();
		}

		if (rightBorderScreenChunk < rightBorderLoadedChunk) {
			rightBorderScreenChunk = Math.Max(leftBorderScreenChunk, rightBorderScreenChunk);

			int amount = Math.Min(rightBorderLoadedChunk - rightBorderScreenChunk, LoadedChunks.ChunkCount);
			await Task.Run(() => LoadedChunks.RemoveLastNChunks(amount));
			// logger.LogDebug($"Right border far away, destroyed {amount} chunks");
			rightBorderLoadedChunk = rightBorderScreenChunk;
			leftBorderLoadedChunk = Math.Min(leftBorderLoadedChunk, rightBorderLoadedChunk);

			_ = Task.Run(plotControl.Refresh);
		}

		rightBorderLock = false;
	}
	#endregion
}
