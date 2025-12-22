using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Points.Services.Database; 
public class SignalMetaEntity {
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int ID { get; set; }

	[Required]
	[MaxLength(200)]
	public string Name { get; set; } = string.Empty;

	[Required]
	public DateTime CreationTime { get; set; }

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
}
