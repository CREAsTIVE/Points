using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Points.Utils;

static class EnumerableChunkExtension {
	public static IEnumerable<IEnumerable<TValue>> EnumerableChunk<TValue>(this IEnumerable<TValue> values, int chunkSize) {
		var enumerator = values.GetEnumerator();
		IEnumerable<TValue> GetChunk() {
			yield return enumerator.Current;
			for (var i = 0; i < chunkSize-1; i++) {
				enumerator.MoveNext();
				yield return enumerator.Current;
			}
		}
		while (enumerator.MoveNext()) {
			yield return GetChunk();
		}
	}
	
	
}
