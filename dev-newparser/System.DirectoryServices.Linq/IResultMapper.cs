
namespace System.DirectoryServices.Linq
{
	public interface IResultMapper
	{
		T Map<T>(SearchResult result);
		object Map(Type type, SearchResult result);
	}
}
