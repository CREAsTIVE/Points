using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Points.Utils; 
public static class IEnumerableUtilsExtension {
	/// <summary>
	/// Iterates over both iterators, ends when both iterators are ended, for completed iterator passes default
	/// </summary>
	public static IEnumerable<(A? a, B? b)> ZipNullable<A, B>(this IEnumerable<A> a, IEnumerable<B> b) {
		var aEnumerator = a.GetEnumerator();
		var bEnumerator = b.GetEnumerator();

		var aMoveNext = aEnumerator.MoveNext();
		var bMoveNext = bEnumerator.MoveNext();
		while (aMoveNext || bMoveNext) {
			yield return (aMoveNext, bMoveNext) switch {
				(true, true) => (aEnumerator.Current, bEnumerator.Current),
				(false, true) => (default, bEnumerator.Current),
				(true, false) => (aEnumerator.Current, default),
				
				_ => throw new UnreachableException()
			};

			aMoveNext = aEnumerator.MoveNext();
			bMoveNext = bEnumerator.MoveNext();
		}
	}
}
