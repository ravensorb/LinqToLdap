using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace System.DirectoryServices.Linq.Expressions {

	// Code from Matt Warren (http://blogs.msdn.com/mattwar/pages/linq-links.aspx)

	public static class Evaluator {
		/// <summary>
		/// Performs evaluation &amp; replacement of independent sub-trees
		/// </summary>
		/// <param name="expression">The root of the expression tree.</param>
		/// <param name="fnCanBeEvaluated">A function that decides whether a given expression node can be part of the local function.</param>
		/// <returns>A new tree with sub-trees evaluated and replaced.</returns>
		public static Expression PartialEval( Expression expression, Func<Expression, bool> fnCanBeEvaluated ) {
			return SubtreeEvaluator.Eval( Nominator.Nominate( fnCanBeEvaluated, expression ), expression );
		}

		/// <summary>
		/// Performs evaluation &amp; replacement of independent sub-trees
		/// </summary>
		/// <param name="expression">The root of the expression tree.</param>
		/// <returns>A new tree with sub-trees evaluated and replaced.</returns>
		public static Expression PartialEval( Expression expression ) {
			return PartialEval( expression, Evaluator.CanBeEvaluatedLocally );
		}

		private static bool CanBeEvaluatedLocally( Expression expression ) {
			return expression.NodeType != ExpressionType.Parameter;
		}

		/// <summary>
		/// Evaluates &amp; replaces sub-trees when first candidate is reached (top-down)
		/// </summary>
		class SubtreeEvaluator : LdapExpressionVisitor {
			HashSet<Expression> candidates;

			private SubtreeEvaluator( HashSet<Expression> candidates ) {
				this.candidates = candidates;
			}

			internal static Expression Eval( HashSet<Expression> candidates, Expression exp ) {
				return new SubtreeEvaluator( candidates ).Visit( exp );
			}

			public override Expression Visit( Expression exp ) {
				if( exp == null ) {
					return null;
				}
				if( this.candidates.Contains( exp ) ) {
					return this.Evaluate( exp );
				}
				return base.Visit( exp );
			}

			private Expression Evaluate( Expression e ) {
				if( e.NodeType == ExpressionType.Constant ) {
					return e;
				}
				Type type = e.Type;
				if( type.IsValueType ) {
					e = Expression.Convert( e, typeof( object ) );
				}
				Expression<Func<object>> lambda = Expression.Lambda<Func<object>>( e );
				Func<object> fn = lambda.Compile();
				return Expression.Constant( fn(), type );
			}
		}

		/// <summary>
		/// Performs bottom-up analysis to determine which nodes can possibly
		/// be part of an evaluated sub-tree.
		/// </summary>
		class Nominator : LdapExpressionVisitor {
			Func<Expression, bool> fnCanBeEvaluated;
			HashSet<Expression> candidates;
			bool cannotBeEvaluated;

			private Nominator( Func<Expression, bool> fnCanBeEvaluated ) {
				this.candidates = new HashSet<Expression>();
				this.fnCanBeEvaluated = fnCanBeEvaluated;
			}

			internal static HashSet<Expression> Nominate( Func<Expression, bool> fnCanBeEvaluated, Expression expression ) {
				Nominator nominator = new Nominator( fnCanBeEvaluated );
				nominator.Visit( expression );
				return nominator.candidates;
			}

			public override Expression Visit( Expression expression ) {
				if( expression != null ) {
					bool saveCannotBeEvaluated = this.cannotBeEvaluated;
					this.cannotBeEvaluated = false;
					base.Visit( expression );
					if( !this.cannotBeEvaluated ) {
						if( this.fnCanBeEvaluated( expression ) ) {
							this.candidates.Add( expression );
						} else {
							this.cannotBeEvaluated = true;
						}
					}
					this.cannotBeEvaluated |= saveCannotBeEvaluated;
				}
				return expression;
			}
		}
	}
}

