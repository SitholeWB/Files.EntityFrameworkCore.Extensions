using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Files.EntityFrameworkCore.Extensions
{
	public static class FilesExtensions
	{
		public static Pagination<TSource> AsPagination<TSource>(this DbContext dbContext, int page, int limit, string sortColumn = "", bool orderByDescending = false) where TSource : class
		{
			PaginationExtensionsHelper.ValidateInputs(page, limit);

			var totalItems = dbContext.Set<TSource>().Count();
			if (!string.IsNullOrEmpty(sortColumn))
			{
				if (orderByDescending)
				{
					var resultsDesc = dbContext.Set<TSource>().OrderByDescending(p => EF.Property<object>(p, sortColumn)).Skip((page - 1) * limit).Take(limit);
					return new Pagination<TSource>(resultsDesc.ToList(), totalItems, page, limit);
				}
				else
				{
					var resultsAsc = dbContext.Set<TSource>().OrderBy(p => EF.Property<object>(p, sortColumn)).Skip((page - 1) * limit).Take(limit);
					return new Pagination<TSource>(resultsAsc.ToList(), totalItems, page, limit);
				}
			}
			var results = dbContext.Set<TSource>().Skip((page - 1) * limit).Take(limit);

			return new Pagination<TSource>(results.ToList(), totalItems, page, limit);
		}
	}
}