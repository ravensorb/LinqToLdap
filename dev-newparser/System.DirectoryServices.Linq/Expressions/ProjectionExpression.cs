using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace System.DirectoryServices.Linq.Expressions {
	public class ProjectionExpression : Expression {
		private readonly DirectorySearcherExpression _searcher;
		private readonly Expression _projector;
		private readonly LambdaExpression _aggregator;

		public ProjectionExpression( DirectorySearcherExpression searcher, Expression projector, LambdaExpression aggregator ) {
			this._searcher = searcher;
			this._projector = projector;
			this._aggregator = aggregator;
		}

		public DirectorySearcherExpression Searcher { get { return this._searcher; } }

		public Expression Projector { get { return this._projector; } }

		public LambdaExpression Aggregator { get { return this._aggregator; } }

		#region Expression details

		public override ExpressionType NodeType {
			get {
				return (ExpressionType)LdapExpressionType.Projection;
			}
		}

		public override Type Type {
			get {
				if( Aggregator != null )
					return Aggregator.Body.Type;
				else
					return typeof( IEnumerable<> ).MakeGenericType( Projector.Type );
			}
		}

		public override string ToString() {
			return string.Format( "Project {0} => {1} {2}", Searcher, Projector, Aggregator );
		}

		#endregion

		#region Update methods

		public ProjectionExpression Update( DirectorySearcherExpression searcher, Expression projector, LambdaExpression aggregator ) {
			if(
				searcher == this.Searcher
				&& projector == this.Projector
				&& aggregator == this.Aggregator
			) {
				return this;
			}
			return new ProjectionExpression( searcher, projector, aggregator );
		}

		public ProjectionExpression Update( DirectorySearcherExpression searcher, Expression projector ) {
			return this.Update( searcher, projector, this.Aggregator );
		}

		public ProjectionExpression Update( DirectorySearcherExpression searcher ) {
			return this.Update( searcher, this.Projector, this.Aggregator );
		}

		#endregion
	}
}
