using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Points.Services.Database {
	public class SignalDbContext(ILogger<SignalDbContext> logger, DbContextOptions<SignalDbContext> options) : DbContext(options) {
		private readonly ILogger logger = logger;

		public DbSet<SignalMetaEntity> SignalsMeta { get; set; } = null!;
		public DbSet<SignalChunkEntity> Chunks { get; set; } = null!;

		protected override void OnModelCreating(ModelBuilder modelBuilder) {
			modelBuilder.Entity<SignalChunkEntity>()
				.HasKey(c => new { c.SignalID, c.ChunkID });

			modelBuilder.Entity<SignalChunkEntity>()
				.HasOne(c => c.SignalMeta)
				.WithMany(m => m.Chunks)
				.HasForeignKey(c => c.SignalID)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<SignalMetaEntity>()
				.Property(m => m.CreationDate)
				.HasConversion(
					v => v.Ticks,
					v => new DateTime(v)
				);
		}
	}
}


 