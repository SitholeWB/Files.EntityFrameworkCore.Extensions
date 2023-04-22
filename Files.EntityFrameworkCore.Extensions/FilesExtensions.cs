using Microsoft.EntityFrameworkCore;
using System;

namespace Files.EntityFrameworkCore.Extensions
{
	public static class FilesExtensions
	{
		public static IFileEntity GetFile<T>(this DbContext dbContext, Guid id) where T : class, IFileEntity
		{
			return dbContext.Set<T>().Find(id);
		}
	}
}