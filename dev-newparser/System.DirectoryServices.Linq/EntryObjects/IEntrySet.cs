using System.Linq;

namespace System.DirectoryServices.Linq.EntryObjects
{
    public interface IEntrySet : IQueryable, IOrderedQueryable
    {
    }

    public interface IEntrySet<T> : IEntrySet, IQueryable<T>, IOrderedQueryable<T> where T : class
    {
        IQueryable<T> CreateQuery();
        void AddEntry(string samAccountName, T entry);
        void DeleteEntry(T entry);
    }
}
