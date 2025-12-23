using Microsoft.EntityFrameworkCore;
using Points.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Windows.Navigation;

namespace Points.Services.Database; 
public class SignalMetaEntity {
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int ID { get; set; }

	[Required]
	[MaxLength(200)]
	public string Name { get; set; } = string.Empty;

	[Required]
	public DateTime CreationDate { get; set; }

	/// <summary>
	/// Time step between points
	/// </summary>
	[Required]
	[Range(double.Epsilon, double.MaxValue)]
	public double TimeStep { get; set; }

	[Required]
	[Range(1, int.MaxValue)]
	public int TotalPoints { get; set; }

	[Required]
	[Range(1, int.MaxValue)]
	public int ChunkSize { get; set; } = 1000;

	// Навигационное свойство для связанных чанков
	public ICollection<SignalChunkEntity> Chunks { get; set; } = new List<SignalChunkEntity>();

	public async Task Update(SignalDbContext db) {
		db.SignalsMeta.Update(this);
		await db.SaveChangesAsync();
	}

	public async Task Update(IDbContextFactory<SignalDbContext> dbFactory) {
		using (var db = await dbFactory.CreateDbContextAsync()) {
			await Update(db);
		}
	}

	public static async Task<SignalMetaEntity> Create(SignalMetaEntity entity, SignalDbContext db) {
		db.SignalsMeta.Add(entity);
		await db.SaveChangesAsync();
		return entity;
	}

	public static async Task<SignalMetaEntity> Create(SignalMetaEntity entity, IDbContextFactory<SignalDbContext> dbFactory) {
		using (var db = await dbFactory.CreateDbContextAsync()) {
			return await Create(entity, db);
		}
	}
}
