using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Points.Utils; 
public class ChunkedList<T> : IReadOnlyList<T> {
	// Maybe use vlist or something
	private List<List<T>> chunks = new();
	public int ChunkCount => chunks.Count;

	private readonly int chunkSize;
	public int ChunkSize => chunkSize;

	public ChunkedList(int chunkSize) {
		if (chunkSize <= 0) throw new ArgumentOutOfRangeException(nameof(chunkSize));
		this.chunkSize = chunkSize;
	}
	
	public T this[int index] {
		get {
			// if (index < 0 || index >= Count) throw new IndexOutOfRangeException();
			return chunks[index / chunkSize][index % chunkSize];
		}
	}

	public int Count => ChunkCount * ChunkSize;

	public void AddLastChunk(List<T> chunk) {
		chunks.Add(chunk);
	}

	public void AddFirstChunk(List<T> chunk) {
		chunks.Insert(0, chunk);
	}

	public void RemoveLastChunk() {
		chunks.RemoveAt(chunks.Count - 1);
	}

	public void RemoveLastNChunks(int count) {
		chunks.RemoveRange(ChunkCount - count, count);
	}

	public void RemoveFirstChunk() {
		chunks.RemoveAt(0);
	}

	public void RemoveFirstNChunks(int count) {
		chunks.RemoveRange(0, count);
	}

	public void Clear() {
		chunks.Clear();
	}

	public IEnumerator<T> GetEnumerator() {
		foreach (var chunk in chunks) {
			foreach (var item in chunk) {
				yield return item;
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
