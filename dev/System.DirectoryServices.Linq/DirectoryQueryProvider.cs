using System.DirectoryServices.Linq.EntryObjects;
using System.Linq;
using System.Linq.Expressions;

namespace System.DirectoryServices.Linq
{
    public class DirectoryQueryProvider : QueryProvider
    {
        public DirectoryQueryProvider(DirectoryContext context)
        {
            Context = context;
        }

        protected DirectoryContext Context { get; private set; }

        /// <summary>
        /// Constructs an <see cref="T:System.Linq.IQueryable"/> object that can evaluate the query represented by a specified expression tree.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Linq.IQueryable"/> that can evaluate the query represented by the specified expression tree.
        /// </returns>
        /// <param name="expression">An expression tree that represents a LINQ query.</param>
        public override IQueryable CreateQuery(Expression expression)
        {
            Type type = GetQueryableType(expression);
            return (IQueryable)Activator.CreateInstance(
                typeof(EntryQuery<>).MakeGenericType(type),
                new object[] { this, CreateQueryState(expression) }
            );
        }

        /// <summary>
        /// Constructs an <see cref="T:System.Linq.IQueryable`1"/> object that can evaluate the query represented by a specified expression tree.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Linq.IQueryable`1"/> that can evaluate the query represented by the specified expression tree.
        /// </returns>
        /// <param name="expression">An expression tree that represents a LINQ query.</param><typeparam name="TElement">The type of the elements of the <see cref="T:System.Linq.IQueryable`1"/> that is returned.</typeparam>
        public override IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new EntryQuery<TElement>(this, CreateQueryState<TElement>(expression));
        }

        /// <summary>
        /// Executes the query represented by a specified expression tree.
        /// </summary>
        /// <returns>
        /// The value that results from executing the specified query.
        /// </returns>
        /// <param name="expression">An expression tree that represents a LINQ query.</param>
        public override object Execute(Expression expression)
        {
            var directoryExpression = CreateQueryState(expression).GetSingleResultExpression();
            return Execute(directoryExpression);
        }

        /// <summary>
        /// Executes the strongly-typed query represented by a specified expression tree.
        /// </summary>
        /// <returns>
        /// The value that results from executing the specified query.
        /// </returns>
        /// <param name="expression">An expression tree that represents a LINQ query.</param><typeparam name="TResult">The type of the value that results from executing the query.</typeparam>
        public override TResult Execute<TResult>(Expression expression)
        {
            var directoryExpression = CreateQueryState<TResult>(expression).GetSingleResultExpression();
            return Execute<TResult>(directoryExpression);
        }

		/// <summary>
		/// Create a new <see cref="EntryQueryState"/> for this provider, based on the given <see cref="Expression"/>.
		/// </summary>
		/// <param name="expression">The expression to get the query state for.</param>
		/// <returns>A new <see cref="EntryQueryState"/> object.</returns>
		protected virtual EntryQueryState CreateQueryState(Expression expression) {
			return new EntryQueryState(Context, expression);
		}

		/// <summary>
		/// Create a new <see cref="EntryQueryState"/> for this provider, based on the given <see cref="Expression"/>.
		/// </summary>
		/// <param name="expression">The expression to get the query state for.</param>
		/// <returns>A new <see cref="EntryQueryState"/> object.</returns>
		protected virtual EntryQueryState CreateQueryState<TElement>(Expression expression) {
			return new EntryQueryState(Context, typeof(TElement), expression);
		}

		/// <summary>
		/// Execute a <see cref="SingleResultExpression"/> expression in the current context.
		/// </summary>
		/// <param name="directoryExpression">The expression to execute.</param>
		/// <returns>The result of executing the expression.</returns>
		protected virtual object Execute(Expressions.SingleResultExpression directoryExpression) {
			return Context.QueryExecutor.Execute(directoryExpression);
		}

		/// <summary>
		/// Execute a <see cref="SingleResultExpression"/> expression in the current context.
		/// </summary>
		/// <param name="directoryExpression">The expression to execute.</param>
		/// <returns>The result of executing the expression.</returns>
		protected virtual TResult Execute<TResult>(Expressions.SingleResultExpression directoryExpression) {
			return Context.QueryExecutor.Execute<TResult>(directoryExpression);
		}

		protected static Type GetQueryableType(Expression expression)
        {
            var methodCall = expression as MethodCallExpression;

            if (methodCall != null)
            {
                var constant = methodCall.Arguments[0] as ConstantExpression;

                if (constant != null && constant.Value != null && typeof(IQueryable).IsAssignableFrom(constant.Type))
                {
                    return ((IQueryable)constant.Value).ElementType;
                }

                return GetQueryableType(methodCall.Arguments[0]);
            }

            Type type = expression.Type;

            if (type.IsGenericType && typeof(IQueryable<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
            {
                return type.GetGenericArguments()[0];
            }

            return type;
        }
    }
}
