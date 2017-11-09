using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace System.DirectoryServices.Linq.EntryObjects {
	public abstract class EntryCollection2 {
	}

	public class EntryCollection2<T> : EntryCollection2, IQueryable<T> where T : class {
		private IQueryProvider _provider;

		public EntryCollection2(IQueryProvider provider) {
			this._provider = provider;
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator() {
			return _provider.CreateQuery<T>( ( (IQueryable)this ).Expression ).GetEnumerator();
		}

		Collections.IEnumerator Collections.IEnumerable.GetEnumerator() {
			return ( (IEnumerable<T>)this ).GetEnumerator();
		}

		Type IQueryable.ElementType {
			get { return typeof( T ); }
		}

		System.Linq.Expressions.Expression IQueryable.Expression {
			get { return Expression.Constant( this ); }
		}

		IQueryProvider IQueryable.Provider {
			get { return _provider; }
		}
	}
}
