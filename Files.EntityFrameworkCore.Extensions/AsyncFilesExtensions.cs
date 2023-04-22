using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Files.EntityFrameworkCore.Extensions
{
	public static class AsyncFilesExtensions
	{
		public static async Task<IFileEntity> GetFileAsync<T>(this DbContext dbContext, Guid id) where T : class, IFileEntity
		{
			return await dbContext.Set<T>().FindAsync(id);
		}
	}
}