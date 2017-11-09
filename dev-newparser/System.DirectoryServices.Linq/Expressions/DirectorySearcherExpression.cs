using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace System.DirectoryServices.Linq.Expressions {
	public class DirectorySearcherExpression : Expression {
		private readonly DirectoryEntry _searchRoot;
		private readonly ReadOnlyCollection<LdapAttributeExpression> _attributesToLoad;
		private readonly SearchScope _scope;
		private readonly Expression _filter;
		private readonly LdapSortExpression _sort;
		private readonly Expression _skip;
		private readonly Expression _take;
		private readonly Type _type;

		internal DirectorySearcherExpression( Type type, DirectoryEntry searchRoot, IEnumerable<LdapAttributeExpression> attributesToLoad, Expression filter, SearchScope scope, LdapSortExpression sort, Expression skip, Expression take ) {
			this._type = type;
			this._scope = scope;
			this._searchRoot = searchRoot;
			var props = attributesToLoad as ReadOnlyCollection<LdapAttributeExpression>;
			if( props == null )
				props = new List<LdapAttributeExpression>( attributesToLoad ).AsReadOnly();
			this._attributesToLoad = props;
			this._filter = filter;
			this._sort = sort;
			this._skip = skip;
			this._take = take;
		}

		public Expression Filter { get { return _filter; } }
		public DirectoryEntry SearchRoot { get { return _searchRoot; } }
		public SearchScope Scope { get { return _scope; } }
		public ReadOnlyCollection<LdapAttributeExpression> AttributesToLoad { get { return _attributesToLoad; } }
		public LdapSortExpression Sort { get { return _sort; } }
		public Expression Skip { get { return _skip; } }
		public Expression Take { get { return _take; } }

		#region Expression details

		public override ExpressionType NodeType {
			get {
				return (ExpressionType)LdapExpressionType.DirectorySearcher;
			}
		}

		public override Type Type {
			get {
				// TODO: is this right, or should it return something like typeof(DirectorySearcher)?
				return _type;
			}
		}

		public override string ToString() {
			return string.Format(
				"DirectorySearcher {0} {1} {2} [{3}] {4} {5} {6}",
				Filter,
				SearchRoot.Name,
				Scope,
				string.Join( ", ", AttributesToLoad ),
				Sort,
				Skip,
				Take
			);
		}

		#endregion

		#region Update methods

		public DirectorySearcherExpression Update( Expression filter, IEnumerable<LdapAttributeExpression> attributesToLoad, LdapSortExpression sort, Expression skip, Expression take ) {
			if(
				filter == this.Filter
				&& attributesToLoad == this.AttributesToLoad
				&& sort == this.Sort
				&& skip == this.Skip
				&& take == this.Take
			) {
				return this;
			} else {
				return new DirectorySearcherExpression(
					this.Type,
					this.SearchRoot,
					attributesToLoad,
					filter,
					this.Scope,
					sort,
					skip,
					take
				);
			}
		}

		public DirectorySearcherExpression Update( Expression filter, IEnumerable<LdapAttributeExpression> attributesToLoad, LdapSortExpression sort ) {
			return this.Update( filter, attributesToLoad, sort, this.Skip, this.Take );
		}

		public DirectorySearcherExpression Update( Expression filter, IEnumerable<LdapAttributeExpression> attributesToLoad ) {
			return this.Update( filter, attributesToLoad, this.Sort, this.Skip, this.Take );
		}

		public DirectorySearcherExpression Update( Expression filter ) {
			return this.Update( filter, this.AttributesToLoad );
		}

		#endregion

	}
}
