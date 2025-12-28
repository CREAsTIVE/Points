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

	public int TotalChunks => TotalPoints / ChunkSize;
	public int LastChunkAmount => TotalPoints % ChunkSize;

	public async Task<List<float>> GetChunk(IDbContextFactory<SignalDbContext> dbFactory, int chunkID) {
		using (var db = await dbFactory.CreateDbContextAsync()) {
			return await GetChunk(db, chunkID);
		}
	}

	/// <returns>Chunk as list of point Y position, <b>without accounting <see cref="TotalPoints"/></b></returns>
	public async Task<List<float>> GetChunk(SignalDbContext db, int chunkID) {
		var result = await db.Chunks
			.Where(chunk => chunk.ChunkID == chunkID && chunk.SignalID == ID)
			.Select(chunk => chunk.Data)
			.FirstAsync();

		return result.Chunk(SignalChunkEntity.PointSize).Select(bytes => BitConverter.ToSingle(bytes)).ToList();
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

		// TODO: Remove overhead by writing it in plain loop
		// TODO: Remove points from last chunk if TotalPoints < chunk size
		return [..result.Select(chunk => chunk.Chunk(SignalChunkEntity.PointSize).Select(bytes => BitConverter.ToSingle(bytes)).ToList())];
	}

	/// <inheritdoc cref="SetPoints(SignalDbContext, IAsyncEnumerable{float}, Func{int, Task}?)"/>
	public async Task SetPoints(IDbContextFactory<SignalDbContext> dbContextFactory, IAsyncEnumerable<float> points, Func<int, Task>? chunkSetUpdate = null) {
		using var db = await dbContextFactory.CreateDbContextAsync();
		await SetPoints(db, points, chunkSetUpdate);
	}

	/// <summary>
	/// Clears and sets all points to a <paramref name="points"/>
	/// </summary>
	/// <param name="points">Finite point enumerable </param>
	/// <param name="chunkSetUpdate">Callback, called when new chunk is created with created chunk ID</param>
	public async Task SetPoints(SignalDbContext db, IAsyncEnumerable<float> points, Func<int, Task>? chunkSetUpdate = null) {
		db.Chunks.RemoveRange(db.Chunks.Where(c => c.SignalID == ID));

		int count = 0; // total amount of points added
		await using var pointsEnumerator = points.GetAsyncEnumerator();
		SignalChunkEntity entity = null!; // Initialized at first iteration

		while (await pointsEnumerator.MoveNextAsync()) {
			if (count % ChunkSize == 0) {
				entity = new() {
					ChunkID = count / ChunkSize,
					SignalID = ID,
					Data = new byte[ChunkSize * SignalChunkEntity.PointSize]
				};

				db.Add(entity);
				_ = chunkSetUpdate?.Invoke(count / ChunkSize);
			}

			var bytes = BitConverter.GetBytes(pointsEnumerator.Current); // TODO: Manual conversation to avoid allocation
			entity.Data[(count % ChunkSize) * SignalChunkEntity.PointSize] = bytes[0];
			entity.Data[(count % ChunkSize) * SignalChunkEntity.PointSize + 1] = bytes[1];
			entity.Data[(count % ChunkSize) * SignalChunkEntity.PointSize + 2] = bytes[2];
			entity.Data[(count % ChunkSize) * SignalChunkEntity.PointSize + 3] = bytes[3];

			count++;
		}

		TotalPoints = count;
		await db.SaveChangesAsync();
	}
}
