using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.DirectoryServices.Linq.Expressions {
	internal class ProjectonReader<T> : IEnumerable<T>, IEnumerable {
		private IEnumerable<T> _internalEnumerable;

		public ProjectonReader(DirectorySearcher searcher, Func<SearchResult, T> projector, int skip) {
			this._internalEnumerable = GetEnumerable(searcher, projector, skip);
		}

		public IEnumerator<T> GetEnumerator() {
			IEnumerable<T> e = this._internalEnumerable;
			if( e == null ) {
				throw new InvalidOperationException( "Cannot enumerate more than once." );
			} else {
				this._internalEnumerable = null;
				return e.GetEnumerator();
			}
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return this.GetEnumerator();
		}

		private static IEnumerable<T> GetEnumerable( DirectorySearcher searcher, Func<SearchResult, T> projector, int skip ) {
			if (searcher == null)
				yield break;
			foreach( SearchResult result in searcher.FindAll().Cast<SearchResult>() ) {
				if( skip > 0 ) {
					skip--;
					continue;
				}
				yield return projector( result );
			}
		}

	}
}
