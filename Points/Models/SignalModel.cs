using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Points.Services.Database;
using Points.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Points.Models;
public class SignalModel(SignalMetaEntity entity) : ObservableObject {
	public readonly SignalMetaEntity entity = entity;

	public int ID => entity.ID;
	public string Name {
		get => entity.Name;
		set => SetProperty(entity.Name, value, entity, (u, n) => u.Name = n);
	}
	public DateTime CreationTime {
		get => entity.CreationDate;
		set => SetProperty(entity.CreationDate, value, entity, (e, v) => e.CreationDate = v);
	}

	public double TimeStep {
		get => entity.TimeStep;
		set => SetProperty(entity.TimeStep, value, entity, (e, v) => e.TimeStep = v);
	}

	public int TotalPoints {
		get => entity.TotalPoints;
		set => SetProperty(entity.TotalPoints, value, entity, (e, v) => e.TotalPoints = v);
	}

	public int ChunkSize {
		get => entity.ChunkSize;
		set => SetProperty(entity.ChunkSize, value, entity, (e, v) => e.ChunkSize = v);
	}

	public int ChunkAmount => TotalPoints / ChunkSize;
	public int LastChunkAmount => TotalPoints % ChunkSize;

	public async Task<List<float>> GetChunk(IDbContextFactory<SignalDbContext> dbFactory, int chunkID) {
		using (var db = await dbFactory.CreateDbContextAsync()) {
			return await GetChunk(db, chunkID);
		}
	}

	public async Task<List<float>> GetChunk(SignalDbContext db, int chunkID) {
		var result = await db.Chunks
			.Where(chunk => chunk.ChunkID == chunkID && chunk.SignalID == ID)
			.Select(chunk => chunk.Data)
			.FirstAsync();

		return result.Chunk(4).Select(bytes => BitConverter.ToSingle(bytes)).ToList();
	}

	public async Task<List<List<float>>> GetChunks(IDbContextFactory<SignalDbContext> dbFactory, int chunkIDFrom, int chunkIDTo) {
		using (var db = await dbFactory.CreateDbContextAsync()) {
			return await GetChunks(db, chunkIDFrom, chunkIDTo);
		}
	}

	public async Task<List<List<float>>> GetChunks(SignalDbContext db, int chunkIDFrom, int chunkIDTo) {
		var result = await db.Chunks
			.Where(chunk => chunk.SignalID == ID && chunk.ChunkID >= chunkIDFrom && chunk.ChunkID < chunkIDTo)
			.OrderBy(chunk => chunk.ChunkID)
			.Select(chunk => chunk.Data)
			.ToListAsync(); // not required?

		// Remove overhead by writing it in plain loop
		return [..result.Select(chunk => chunk.Chunk(4).Select(bytes => BitConverter.ToSingle(bytes)).ToList())]; 
	}

	public async Task SetChunks(IDbContextFactory<SignalDbContext> dbContextFactory, ICollection<float> points) {
		using (var db = await dbContextFactory.CreateDbContextAsync()) {
			await SetChunks(db, points);
		}
	}
	public async Task SetChunks(SignalDbContext db, ICollection<float> points) {
		db.Chunks.AddRange(
			points.EnumerableChunk(ChunkSize).Select((chunk, index) => new SignalChunkEntity() {
				SignalID = ID,
				ChunkID = index,
				Data = chunk.Select(f => BitConverter.GetBytes(f)).SelectMany(v => v).ToArray()
			})
		);

		TotalPoints = points.Count;
		await entity.Update(db);

		await db.SaveChangesAsync();
	}
}
