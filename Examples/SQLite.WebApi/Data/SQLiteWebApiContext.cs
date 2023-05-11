using Microsoft.EntityFrameworkCore;
using SQLite.WebApi.Entities;

namespace SQLite.WebApi.Data
{
	public class SQLiteWebApiContext : DbContext
	{
		public SQLiteWebApiContext(DbContextOptions<SQLiteWebApiContext> options)
			: base(options)
		{
			Database.Migrate();
		}

		public DbSet<FoodImage> FoodImage { get; set; } = default!;
	}
}