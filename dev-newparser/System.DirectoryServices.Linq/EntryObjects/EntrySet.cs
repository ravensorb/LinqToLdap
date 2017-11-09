using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace System.DirectoryServices.Linq.EntryObjects
{
    public abstract class EntrySet : IEntrySet
    {
        #region Constructors

        internal EntrySet(DirectoryContext context)
        {
            Context = context;
        }

        #endregion

        #region Properties

        internal DirectoryContext Context { get; private set; }

		internal virtual DirectoryEntry RootEntry { get { return Context.RootEntry; } }

		internal virtual SearchScope? Scope { get { return null; } }

        Type IQueryable.ElementType
        {
            get
            {
                return GetElementType();
            }
        }

        Expression IQueryable.Expression
        {
            get
            {
                return GetExpression();
            }
        }

        IQueryProvider IQueryable.Provider
        {
            get
            {
                return GetProvider();
            }
        }

        #endregion

        #region Methods

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumeratorCore();
        }

        protected virtual IQueryProvider GetProvider()
        {
            return Context.QueryProvider;
        }

        protected abstract Type GetElementType();

        protected abstract Expression GetExpression();

        protected abstract IEnumerator GetEnumeratorCore();

        #endregion
    }

    public class EntrySet<T> : EntrySet, IEntrySet<T>, IOrderedQueryable<T> where T : class
    {
        #region Constructors

		public EntrySet(DirectoryContext context) : base(context)
        {
        }

        #endregion

        #region Methods

        protected override Type GetElementType()
        {
            return typeof(T);
        }

        protected override Expression GetExpression()
        {
            return Expression.Constant(this);
        }

        protected override IEnumerator GetEnumeratorCore()
        {
            return GetEnumerator();
        }

		protected virtual IQueryable<T> CreateQuery()
		{
			return GetProvider().CreateQuery<T>(GetExpression());
		}

		IQueryable<T> IEntrySet<T>.CreateQuery()
		{
			return CreateQuery();
		}

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return CreateQuery().GetEnumerator();
        }

        public virtual void AddEntry(string samAccountName, T entry)
        {
            var entryObject = entry as EntryObject;

            if (entryObject != null)
            {
                Context.AddObject<T>(samAccountName, entry);
            }
        }

        public void DeleteEntry(T entry)
        {
            var entryObject = entry as EntryObject;

            if (entryObject != null)
            {
                Context.DeleteObject<T>(entry);
            }
        }

        #endregion
    }
}
