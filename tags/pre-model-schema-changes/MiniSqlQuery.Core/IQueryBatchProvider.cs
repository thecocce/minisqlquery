using System;

namespace MiniSqlQuery.Core
{
	/// <summary>
	/// A query batch provider is a class (typically a window) that manages a 
	/// batch query and therefore has a data result etc.
	/// </summary>
	public interface IQueryBatchProvider
	{
		/// <summary>
		/// Gets a reference to the batch.
		/// </summary>
		/// <value>The query batch.</value>
		QueryBatch Batch { get; }
	}
}