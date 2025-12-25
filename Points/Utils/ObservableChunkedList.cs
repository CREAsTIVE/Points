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
public class ObservableChunkedList<T> : IReadOnlyList<T>, INotifyCollectionChanged, INotifyPropertyChanged {
	private readonly ObservableCollection<List<T>> chunks = new();
	public int ChunkCount => chunks.Count;

	private readonly int chunkSize;
	public int ChunkSize => chunkSize;

	public ObservableChunkedList(int chunkSize) {
		if (chunkSize <= 0) throw new ArgumentOutOfRangeException(nameof(chunkSize));
		this.chunkSize = chunkSize;
		chunks.CollectionChanged += OnChunksCollectionChanged;
	}
	
	public T this[int index] {
		get {
			if (index < 0 || index >= Count) throw new IndexOutOfRangeException();
			return chunks[index / chunkSize][index % chunkSize];
		}
	}

	public int Count => ChunkCount * ChunkSize;

	public void AddLastChunk(List<T> chunk) {
		if (chunk == null || chunk.Count > chunkSize) throw new ArgumentException("Invalid chunk");
		chunks.Add(chunk);
	}

	public void AddFrontChunk(List<T> chunk) {
		if (chunk == null || chunk.Count > chunkSize) throw new ArgumentException("Invalid chunk");
		chunks.Insert(0, chunk);
	}

	public void RemoveLastChunk() {
		if (chunks.Count == 0) return;
		chunks.RemoveAt(chunks.Count - 1);
	}

	public void RemoveFrontChunk() {
		if (chunks.Count == 0) return;
		chunks.RemoveAt(0);
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

	public event NotifyCollectionChangedEventHandler? CollectionChanged;
	public event PropertyChangedEventHandler? PropertyChanged;

	private void OnChunksCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
		NotifyCollectionChangedEventArgs args = e.Action switch {
			// Better (e.NewItems as List<List<T>>).First()
			NotifyCollectionChangedAction.Add => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, e.NewItems?[0], e.NewStartingIndex * chunkSize),
			NotifyCollectionChangedAction.Remove => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, e.OldItems?[0], e.OldStartingIndex * chunkSize),
			NotifyCollectionChangedAction.Reset => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset),

			// Warning! Not implemented
			NotifyCollectionChangedAction.Replace => e,
			NotifyCollectionChangedAction.Move => e,

			_ => throw new NotImplementedException(), // Warn supression
		};

		CollectionChanged?.Invoke(this, args);

		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
	}
}
