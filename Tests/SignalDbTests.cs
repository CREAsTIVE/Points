using Microsoft.EntityFrameworkCore;
using NLog;
using Points.Models;
using Points.Services.Database;
using System;
using System.Collections.Generic;
using System.Text;
using Tests.Utils;
using Xunit.Abstractions;

namespace Tests; 
public class SignalDbTests(ITestOutputHelper output) : IDisposable {
	IDbContextFactory<SignalDbContext> dbFactory = new DatabaseContextFactory<SignalDbContext>(
		() => new SignalDbContext(
			new TestsLogger<SignalDbContext>(output), 
			new DbContextOptionsBuilder<SignalDbContext>().UseInMemoryDatabase("TestDB").Options)
		);

	[Fact]
	public async Task SaveLoadSignalMeta() {
		using (var db = dbFactory.CreateDbContext()) {
			// db.Database.OpenConnection(); // for mysql in-memory testing

			await db.Database.EnsureCreatedAsync();

			var creationTime = new DateTime(2010, 1, 1, 1, 1, 1);
			var signalMeta = new SignalMetaEntity() {
				Name = "TestName",
				CreationTime = creationTime,
				ChunkSize = 1000
			};

			await SignalMetaEntity.Create(signalMeta, db);

			var signal1Copy = db.SignalsMeta.Where(meta => meta.Name == signalMeta.Name).First();

			Assert.Equivalent(signal1Copy, signalMeta);

			var signal2 = await SignalMetaEntity.Create(new() {
				Name = "SecondSignal",
				CreationTime = creationTime,
			}, db);

			var amount = db.SignalsMeta.Where(meta => meta.CreationTime == creationTime).Count();

			Assert.True(amount == 2);
		}
	}

	public void Dispose() {

	}
}
