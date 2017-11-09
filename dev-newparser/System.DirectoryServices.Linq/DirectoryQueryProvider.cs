using System.DirectoryServices.Linq.EntryObjects;
using System.DirectoryServices.Linq.Expressions;
using System.Linq;
using System.Linq.Expressions;

namespace System.DirectoryServices.Linq {
	public class DirectoryQueryProvider : QueryProvider {
		public DirectoryQueryProvider( DirectoryContext context ) {
			Context = context;
		}

		protected DirectoryContext Context { get; private set; }

		public string GetFilterText( Expression expression ) {
			ProjectionExpression projection = this.Translate( expression );
			if( projection == null || projection.Searcher == null )
				return null;
			return LdapFilterFormatter.Format( projection.Searcher.Filter );
		}

		public override object Execute( Expression expression ) {
			ProjectionExpression projection = this.Translate( expression );
			var projector = ProjectionBuilder.Build(Context, projection.Projector );
			DirectorySearcher searcher = CreateSearcher( projection.Searcher );
			var reader = GetReader( projector, projection.Aggregator, true, ( projection.Searcher == null ) ? null : projection.Searcher.Skip );
			return ( (Func<DirectorySearcher, object>)reader.Compile() )( searcher );
		}

		private static LambdaExpression GetReader( LambdaExpression projector, LambdaExpression aggregator, bool box, Expression skip ) {
			ParameterExpression searchResults = Expression.Parameter( typeof( DirectorySearcher ), "searcher" );
			if( skip == null )
				skip = Expression.Constant( 0 );
			Type type = projector.Body.Type;
			Expression body = Expression.New(
				typeof(ProjectonReader<>).MakeGenericType(type).GetConstructors()[0],
				searchResults,
				projector,
				skip
			);
			if( aggregator != null )
				body = Expression.Invoke( aggregator, body );
			if (box) {
				body = Expression.Convert( body, typeof( object ) );
			}
			return Expression.Lambda( body, searchResults );
		}

		private DirectorySearcher CreateSearcher( DirectorySearcherExpression node ) {
			if( node == null )
				return null;
			DirectoryEntry searchRoot = node.SearchRoot;
			string filter = LdapFilterFormatter.Format( node.Filter );
			if( string.IsNullOrEmpty( filter ) )
				filter = null;
			string[] propertiesToLoad = node.AttributesToLoad.Select( a => a.Name ).ToArray();
			SearchScope scope = node.Scope;
			SortOption sort = CreateSortOption( node.Sort );
			int? sizeLimit = null;
			if (node.Take != null) {
				var take = node.Take as ConstantExpression;
				if (take == null)
					throw new NotSupportedException("Cannot have a take value of anything other than a constant.");
				sizeLimit = (int)take.Value;
				// Have to add on the skip size, so we don't get too few results.
				var skip = node.Skip as ConstantExpression;
				if (skip != null) {
					sizeLimit += (int)skip.Value;
				}
			}

			var searcher = new DirectorySearcher( searchRoot, filter, propertiesToLoad, scope );
			if( sort != null )
				searcher.Sort = sort;
			if (sizeLimit.HasValue)
				searcher.SizeLimit = sizeLimit.Value;

			return searcher;
		}

		private SortOption CreateSortOption( LdapSortExpression node ) {
			if( node == null )
				return null;

			var attributeExpression = node.Expression as LdapAttributeExpression;
			if( attributeExpression == null )
				throw new NotSupportedException( "Cannot have a sort order other than a single property." );

			var option = new SortOption(
				attributeExpression.Name,
				node.Direction
			);

			// For some reason, setting it in the constructor doesn't seem to work.
			option.Direction = node.Direction;

			return option;
		}

		private ProjectionExpression Translate( Expression expression ) {
			expression = Evaluator.PartialEval( expression, CanBeEvaluatedLocally );
			expression = QueryBinder.Bind( this, expression );
			expression = SubstringExpressionRewriter.Rewrite( expression  );
			expression = SubstringEmptyClauseRemover.Rewrite( expression );
			expression = BinaryExpressionRewriter.Rewrite( expression );
			expression = AttributeComparisonReorderingRewriter.Rewrite( expression );
			expression = NullComparisonRewriter.Rewrite( expression );
			expression = AndOrExpressionCollector.Rewrite( expression );
			expression = RedundantExpressionRemover.Rewrite( expression );
			return (ProjectionExpression)expression;
		}

		private bool CanBeEvaluatedLocally( Expression expression ) {
			// any operation on a query can't be done locally
			ConstantExpression cex = expression as ConstantExpression;
			if( cex != null ) {
				IQueryable query = cex.Value as IQueryable;
				if( query != null && query.Provider == this )
					return false;
			}
			MethodCallExpression mc = expression as MethodCallExpression;
			if( mc != null &&
				( mc.Method.DeclaringType == typeof( Enumerable ) ||
				 mc.Method.DeclaringType == typeof( Queryable ) ) ) {
				return false;
			}
			if( expression.NodeType == ExpressionType.Convert &&
				expression.Type == typeof( object ) )
				return true;
			return expression.NodeType != ExpressionType.Parameter &&
				   expression.NodeType != ExpressionType.Lambda;
		}

	}
}
