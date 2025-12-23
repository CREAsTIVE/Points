using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Points.Utils;

public class ObservableLinkedList<T> : INotifyCollectionChanged, IEnumerable<T> {
	private LinkedList<T> underLyingLinkedList;

	#region Variables accessors
	public int Count {
		get { return underLyingLinkedList.Count; }
	}

	public LinkedListNode<T>? First {
		get { return underLyingLinkedList.First; }
	}

	public LinkedListNode<T>? Last {
		get { return underLyingLinkedList.Last; }
	}
	#endregion

	#region Constructors
	public ObservableLinkedList() {
		underLyingLinkedList = new LinkedList<T>();
	}

	public ObservableLinkedList(IEnumerable<T> collection) {
		underLyingLinkedList = new LinkedList<T>(collection);
	}
	#endregion

	#region LinkedList<T> Composition
	public LinkedListNode<T> AddAfter(LinkedListNode<T> prevNode, T value) {
		LinkedListNode<T> ret = underLyingLinkedList.AddAfter(prevNode, value);
		OnNotifyCollectionChanged();
		return ret;
	}

	public void AddAfter(LinkedListNode<T> node, LinkedListNode<T> newNode) {
		underLyingLinkedList.AddAfter(node, newNode);
		OnNotifyCollectionChanged();
	}

	public LinkedListNode<T> AddBefore(LinkedListNode<T> node, T value) {
		LinkedListNode<T> ret = underLyingLinkedList.AddBefore(node, value);
		OnNotifyCollectionChanged();
		return ret;
	}

	public void AddBefore(LinkedListNode<T> node, LinkedListNode<T> newNode) {
		underLyingLinkedList.AddBefore(node, newNode);
		OnNotifyCollectionChanged();
	}

	public LinkedListNode<T> AddFirst(T value) {
		LinkedListNode<T> ret = underLyingLinkedList.AddFirst(value);
		OnNotifyCollectionChanged();
		return ret;
	}

	public void AddFirst(LinkedListNode<T> node) {
		underLyingLinkedList.AddFirst(node);
		OnNotifyCollectionChanged();
	}

	public LinkedListNode<T> AddLast(T value) {
		LinkedListNode<T> ret = underLyingLinkedList.AddLast(value);
		OnNotifyCollectionChanged();
		return ret;
	}

	public void AddLast(LinkedListNode<T> node) {
		underLyingLinkedList.AddLast(node);
		OnNotifyCollectionChanged();
	}

	public void Clear() {
		underLyingLinkedList.Clear();
		OnNotifyCollectionChanged();
	}

	public bool Contains(T value) {
		return underLyingLinkedList.Contains(value);
	}

	public void CopyTo(T[] array, int index) {
		underLyingLinkedList.CopyTo(array, index);
	}

	public bool LinkedListEquals(object obj) {
		return underLyingLinkedList.Equals(obj);
	}

	public LinkedListNode<T>? Find(T value) {
		return underLyingLinkedList.Find(value);
	}

	public LinkedListNode<T>? FindLast(T value) {
		return underLyingLinkedList.FindLast(value);
	}

	public Type GetLinkedListType() {
		return underLyingLinkedList.GetType();
	}

	public bool Remove(T value) {
		bool ret = underLyingLinkedList.Remove(value);
		OnNotifyCollectionChanged();
		return ret;
	}

	public void Remove(LinkedListNode<T> node) {
		underLyingLinkedList.Remove(node);
		OnNotifyCollectionChanged();
	}

	public void RemoveFirst() {
		underLyingLinkedList.RemoveFirst();
		OnNotifyCollectionChanged();
	}

	public void RemoveLast() {
		underLyingLinkedList.RemoveLast();
		OnNotifyCollectionChanged();
	}
	#endregion

	#region INotifyCollectionChanged Members

	public event NotifyCollectionChangedEventHandler? CollectionChanged;
	public void OnNotifyCollectionChanged() {
		if (CollectionChanged != null) {
			CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}
	}

	#endregion

	#region IEnumerable Members

	IEnumerator<T> IEnumerable<T>.GetEnumerator() {
		return underLyingLinkedList.GetEnumerator();
	}

	public IEnumerator GetEnumerator() {
		return underLyingLinkedList.GetEnumerator();
	}

	#endregion
}
