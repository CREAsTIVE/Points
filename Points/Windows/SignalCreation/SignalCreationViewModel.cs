using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Points.Models;
using Points.Services.Database;
using Points.Windows.SignalCreation.Presets;
using Points.Windows.SignalView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Points.Windows.SignalCreation; 
public partial class SignalCreationViewModel(IDbContextFactory<SignalDbContext> dbFactory, Func<SignalViewWindow> signalViewWindowFactory) : ObservableObject {
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

	[RelayCommand]
	public async Task CreateSignal() {
		CurrentSignalModel.CreationTime = DateTime.Now;
		var result = await SignalMetaEntity.Create(CurrentSignalModel.entity, dbFactory);
		// await CurrentSignalModel.SetChunks(dbFactory, Enumerable.Range(0, CurrentSignalModel.TotalPoints).Select(_ => Random.Shared.NextSingle()*2-1).ToList());
		await CurrentSignalModel.SetPoints(dbFactory, SelectedPreset.GetPoints((float)CurrentSignalModel.TimeStep, CurrentSignalModel.TotalPoints), CurrentSignalModel.TotalPoints);

		var window = signalViewWindowFactory();
		((SignalViewViewModel)window.DataContext).SetSignal(new(result));
		window.Show();

		baseWindow?.Created();
	}
}
