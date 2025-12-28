using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Points.Models;
using Points.Services.Database;
using Points.Windows.SignalCreation.Presets;
using Points.Windows.SignalViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Points.Windows.SignalCreation; 
public partial class SignalCreationViewModel(IDbContextFactory<SignalDbContext> dbFactory, Func<SignalViewerWindow> signalViewWindowFactory) : ObservableObject {
	[ObservableProperty]
	SignalModel currentSignalModel = new(new() {
		Name = "Новый сигнал",
		CreationDate = DateTime.Now,
		TimeStep = 0.001,
		TotalPoints = 10000
	});

	// Bad practice, but i don't want to suffer with unacessable window Close and Open methods
	public SignalCreationWindow? baseWindow;

	[ObservableProperty]
	List<IGenerationPreset> presets = IGenerationPreset.Common;

	[ObservableProperty]
	IGenerationPreset selectedPreset = IGenerationPreset.Common[0];

	[ObservableProperty]
	int creationProgress = 0;

	[ObservableProperty]
	string creationState = "";

	[ObservableProperty]
	int totalChunks = 1;

	[RelayCommand]
	public async Task CreateSignal() {
		CurrentSignalModel.CreationDate = DateTime.Now;
		var result = await SignalMetaEntity.Create(CurrentSignalModel.entity, dbFactory);
		// await CurrentSignalModel.SetChunks(dbFactory, Enumerable.Range(0, CurrentSignalModel.TotalPoints).Select(_ => Random.Shared.NextSingle()*2-1).ToList());

		TotalChunks = CurrentSignalModel.TotalChunks;
		await Task.Run(async () => {
			CreationState = "Генерация точек";
			await CurrentSignalModel.SetPoints(dbFactory, SelectedPreset.GetPoints((float)CurrentSignalModel.TimeStep, CurrentSignalModel.TotalPoints), async (amount) => {
				await Application.Current.Dispatcher.InvokeAsync(() => {
					CreationProgress = amount;
					if (CreationProgress == TotalChunks - 1) {
						CreationState = "Сохранение";
					}
				});
			});
		});

		CreationState = "Открытие";

		baseWindow?.Created(new(result));
	}
}
