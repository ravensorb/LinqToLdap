using System.DirectoryServices.Linq.Attributes;
using System.DirectoryServices.Linq.Expressions;
using System.DirectoryServices.Linq.Filters;
using System.Linq.Expressions;
using System.Reflection;

namespace System.DirectoryServices.Linq
{
    public class QueryTranslator : IQueryTranslator
    {
        #region Constructors

        public QueryTranslator(DirectoryContext context)
        {
            Context = context;
        }

        #endregion

        #region Properties

        public DirectoryContext Context { get; private set; }

        public IResultMapper ResultMapper
        {
            get
            {
                return Context.ResultMapper;
            }
        }

        #endregion

        #region Methods

        public T TranslateOne<T>(TranslatorContext context)
        {
            var result = TranslateOne(context);
            var select = context.Expression.Select;

			if (!(result is int) && select != null)
            {
                var origionalType = context.Expression.GetOrigionalType();

                if (origionalType != typeof(T))
                {
                    var projection = select.Projection.Compile();
                    result = projection.DynamicInvoke(result);
                }
            }

            return (T)result;
        }

        public T TranslateOne<T>(DirectorySearcher searcher)
        {
            var context = new TranslatorContext(searcher);
            return ResultMapper.Map<T>(context.FindOne());
        }

        public T TranslateOne<T>(SingleResultExpression expression, DirectorySearcher searcher)
        {
            return TranslateOne<T>(new TranslatorContext(expression, searcher));
        }

        public object TranslateOne(TranslatorContext context)
        {
            var directorySearch = context.DirectorySearcher;
            var origionalType = context.Expression.GetOrigionalType();
            directorySearch.Filter = Parse(context).ToString();

            if (context.Expression.NodeType.Is(DirectoryExpressionType.SingleResult))
            {
                var singleResultExpression = (SingleResultExpression)context.Expression;

                if (singleResultExpression.SingleResultType == SingleResultType.Count)
                {
                    return ((SearchResults)context.FindAll()).Count;
                }
            }

            return ResultMapper.Map(origionalType, context.FindOne());
        }

        public object TranslateOne(Type elementType, DirectorySearcher searcher)
        {
            var context = new TranslatorContext(searcher);
            return ResultMapper.Map(elementType, context.FindOne());
        }

        public object TranslateOne(SingleResultExpression expression, DirectorySearcher searcher)
        {
            return TranslateOne(new TranslatorContext(expression, searcher));
        }

        public DirectoryEnumerator<T> Translate<T>(TranslatorContext context)
        {
            var directorySearch = context.DirectorySearcher;
            directorySearch.Filter = Parse(context).ToString();
            return new DirectoryEnumerator<T>(context, ResultMapper, context.FindAll());
        }

        public DirectoryEnumerator<T> Translate<T>(DirectorySearcher searcher)
        {
            var context = new TranslatorContext(searcher);
            return new DirectoryEnumerator<T>(context, ResultMapper, context.FindAll());
        }

        public DirectoryEnumerator<T> Translate<T>(DirectoryExpression expression, DirectorySearcher directorySearcher)
        {
            return Translate<T>(new TranslatorContext(expression, directorySearcher));
        }

        private static FilterBuilder Parse(TranslatorContext context)
        {
            var expression = context.Expression;
            var builder = new FilterBuilder(expression.GetOrigionalType());

            ParseWhereClause(builder, expression);
            ParseOrderBy(context, expression.OrderBy);

            return builder;
        }

        private static void ParseWhereClause(FilterBuilder builder, DirectoryExpression expression)
        {
            var type = expression.GetOrigionalType();
            var attributeBuilder = builder.CreateBuilder();
            attributeBuilder.AddObjectClass(GetName(type));

            foreach (var where in expression.WhereClause)
            {
                VisitExpression(attributeBuilder, where);
            }

            builder.AddBuilder(attributeBuilder);
        }

        private static void ParseOrderBy(TranslatorContext context, OrderByExpression orderBy)
        {
            if (orderBy != null)
            {
                var sortOption = context.DirectorySearcher.Sort;
                sortOption.Direction = (SortDirection)orderBy.Direction;
                sortOption.PropertyName = GetName(orderBy.OrderByProperty.Member);
            }
        }

        private static void VisitExpression(AttributeBuilder builder, Expression expression)
        {
            if (expression.NodeType.Is(DirectoryExpressionType.Where))
            {
                VisitLambda(builder, ((WhereExpression)expression).Where);
            }
            else if (expression.NodeType.Is(ExpressionType.Lambda))
            {
                VisitLambda(builder, (LambdaExpression)expression);
            }
            else if (expression is BinaryExpression)
            {
                VisitBinary(builder, (BinaryExpression)expression);
            }
            else if (expression.NodeType.Is(ExpressionType.MemberAccess))
            {
                VisitMember(builder, (MemberExpression)expression);
            }
            else if (expression.NodeType.Is(ExpressionType.Constant))
            {
                VisitConstant(builder, (ConstantExpression)expression);
            }
            else if (expression.NodeType.Is(ExpressionType.Call))
            {
                VisitMethod(builder, (MethodCallExpression)expression);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private static void VisitLambda(AttributeBuilder builder, LambdaExpression lambda)
        {
            if (lambda.Body is BinaryExpression)
            {
                VisitBinary(builder, (BinaryExpression)lambda.Body);
            }
            else if (lambda.Body is MethodCallExpression)
            {
                VisitMethod(builder, (MethodCallExpression)lambda.Body);
            }
        }

        private static void VisitMember(AttributeBuilder builder, MemberExpression member)
        {
        }

        private static void VisitConstant(AttributeBuilder builder, ConstantExpression constant)
        {
        }

        private static void VisitBinary(AttributeBuilder builder, BinaryExpression binary)
        {
            switch (binary.NodeType)
            {
                case ExpressionType.AndAlso:
                    {
                        builder.Add(Filter.OpenAndGroup);
                        VisitExpression(builder, binary.Left);
                        VisitExpression(builder, binary.Right);
                        builder.Add(Filter.CloseGroup);
                        break;
                    }
                case ExpressionType.OrElse:
                    {
                        builder.Add(Filter.OpenOrGroup);
                        VisitExpression(builder, binary.Left);
                        VisitExpression(builder, binary.Right);
                        builder.Add(Filter.CloseGroup);
                        break;
                    }
                case ExpressionType.Equal:
                    {
                        VisitBinaryMemberAccess(builder, binary, FilterOperator.Equals);
                        break;
                    }
                case ExpressionType.NotEqual:
                    {
                        VisitBinaryMemberAccess(builder, binary, FilterOperator.NotEquals);
                        break;
                    }
                case ExpressionType.GreaterThan:
                    {
                        VisitBinaryMemberAccess(builder, binary, FilterOperator.GreaterThan);
                        break;
                    }
                case ExpressionType.GreaterThanOrEqual:
                    {
                        VisitBinaryMemberAccess(builder, binary, FilterOperator.GreaterThanOrEqual);
                        break;
                    }
                case ExpressionType.LessThan:
                    {
                        VisitBinaryMemberAccess(builder, binary, FilterOperator.LessThan);
                        break;
                    }
                case ExpressionType.LessThanOrEqual:
                    {
                        VisitBinaryMemberAccess(builder, binary, FilterOperator.LessThanOrEqual);
                        break;
                    }
                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }

        private static void VisitBinaryMemberAccess(AttributeBuilder builder, BinaryExpression binary, FilterOperator filterOperator)
        {
            Expression valueExpression = null;
            var memberExpression = GetMemberAccessExpression(builder, binary.Left);

            if (memberExpression == null)
            {
                valueExpression = binary.Left;

                memberExpression = GetMemberAccessExpression(builder, binary.Right);

				if (memberExpression == null)
				{
					// no member was found..so assume one of the values is an attribute name.
					var attributeName = Expression.Lambda(binary.Left).Compile().DynamicInvoke();
					var attributeValue = Expression.Lambda(binary.Right).Compile().DynamicInvoke();
					builder.AddAttribute(Convert.ToString(attributeName), filterOperator, EscapeCharacters(attributeValue));
					return;
				}
            }
            else
            {
                valueExpression = binary.Right;
            }

            var value = Expression.Lambda(valueExpression).Compile().DynamicInvoke();
            builder.AddAttribute(GetName(memberExpression.Member), filterOperator, EscapeCharacters(value));
        }

        private static object EscapeCharacters(object value)
        {
            if (value != null && value is string)
            {
                return Convert.ToString(value).Replace("(", "0x28").Replace(")", "0x29").Replace("\\", "0x5c").Replace(",", "\\,");
            }

            return value;
        }

        private static MemberExpression GetMemberAccessExpression(AttributeBuilder builder, Expression expression)
        {
            if (expression.NodeType.Is(ExpressionType.MemberAccess))
            {
                // Commented out by Stephen for the purpose of an anonomous object in the query.
                // Example:
                // var query = new { Cn = string.Empty };
                // Users.Where(u => query.Cn == "My Name");

                //MemberExpression memberExpression = (MemberExpression)expression;

                //if (memberExpression.Member.DeclaringType == builder.Parent.GetObjectClassType())
                //{
                //    return memberExpression;
                //}

                // End comment by Stephen

                return (MemberExpression)expression;
            }

            return null;
        }

        private static void VisitMethod(AttributeBuilder builder, MethodCallExpression method)
        {
            var methodInfo = method.Method;

			if (methodInfo.DeclaringType == typeof(string) && (methodInfo.CallingConvention & CallingConventions.HasThis) == CallingConventions.HasThis)
            {
                switch (methodInfo.Name)
                {
                    case "StartsWith":
                        {
                            if (method.Arguments.Count == 1)
                            {
                                VisitMethodCall(builder, method, method.Arguments[0], FilterOperator.StartsWith);
                            }
                            break;
                        }
                    case "EndsWith":
                        {
                            if (method.Arguments.Count == 1)
                            {
                                VisitMethodCall(builder, method, method.Arguments[0], FilterOperator.EndsWith);
                            }
                            break;
                        }
                    case "Contains":
                        {
                            if (method.Arguments.Count == 1)
                            {
                                VisitMethodCall(builder, method, method.Arguments[0], FilterOperator.Contains);
                            }
                            break;
                        }
                    default:
                        {
                            throw new NotSupportedException(string.Format(Properties.Resources.MethodNotSupported, methodInfo.Name));
                        }
                }
            }
        }

        private static void VisitMethodCall(AttributeBuilder builder, MethodCallExpression method, Expression valueExpression, FilterOperator filterOperator)
        {
            var value = Expression.Lambda(valueExpression).Compile().DynamicInvoke();
            var memberExpression = GetMemberAccessExpression(builder, method.Object);
            builder.AddAttribute(GetName(memberExpression.Member), filterOperator, value);
        }

        private static string GetName(MemberInfo info)
        {
            var typeAttribute = info.GetAttribute<DirectoryTypeAttribute>();

            if (typeAttribute != null)
            {
                return typeAttribute.Name;
            }

            var propertyAttribute = info.GetAttribute<DirectoryPropertyAttribute>();

            if (propertyAttribute != null)
            {
                return propertyAttribute.Name;
            }

            return info.Name;
        }

        #endregion
    }
}
