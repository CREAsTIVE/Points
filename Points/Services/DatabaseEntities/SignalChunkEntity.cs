using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Points.Services.Database; 
public class SignalChunkEntity {
	// Warn: do NOT change that without changing database handling
	/// <summary>
	/// Amount of bytes one point occupies
	/// </summary>
	public const int PointSize = sizeof(float);

	[Key]
	[Column(Order = 0)]
	public int SignalID { get; set; }

	[Key]
	[Column(Order = 1)]
	[Range(0, int.MaxValue)]
	public int ChunkID { get; set; }

	[Required]
	public byte[] Data { get; set; } = Array.Empty<byte>(); // Сериализованные данные точек

	// Навигационное свойство
	[ForeignKey("SignalID")]
	public SignalMetaEntity SignalMeta { get; set; } = null!;
}
