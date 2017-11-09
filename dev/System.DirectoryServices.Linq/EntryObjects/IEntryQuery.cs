using System;

namespace System.DirectoryServices.Linq.EntryObjects
{
	public interface IEntryQuery
	{
		DirectoryContext Context { get; }

		EntryQueryState QueryState { get; }
	}

	public interface IEntryQuery<T> : IEntryQuery
	{
	}
}
