using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Points.Models;
using Points.Services.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Points.Windows.SignalCreation; 
public partial class SignalCreationViewModel(IDbContextFactory<SignalDbContext> dbFactory) : ObservableObject {
	[ObservableProperty]
	SignalModel currentSignalModel = new(new() {
		Name = "Новый сигнал",
		CreationDate = DateTime.Now,
		TimeStep = 0.001,
		TotalPoints = 10000
	});

	// Bad practice, but i don't want to suffer with unacessable window Close and Open methods
	public SignalCreationWindow? baseWindow;

	[RelayCommand]
	public async Task CreateSignal() {
		CurrentSignalModel.CreationTime = DateTime.Now;
		var result = await SignalMetaEntity.Create(CurrentSignalModel.entity, dbFactory);
		await CurrentSignalModel.SetChunks(dbFactory, Enumerable.Range(0, CurrentSignalModel.TotalPoints).Select(_ => Random.Shared.NextSingle()*2-1).ToList());

		baseWindow?.Created();
	}
}
