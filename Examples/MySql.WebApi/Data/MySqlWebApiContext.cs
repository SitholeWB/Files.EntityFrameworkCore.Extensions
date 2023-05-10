using Microsoft.EntityFrameworkCore;

namespace MySql.WebApi.Data
{
	public class MySqlWebApiContext : DbContext
	{
		public MySqlWebApiContext(DbContextOptions<MySqlWebApiContext> options)
			: base(options)
		{
			Database.Migrate();
		}

		public DbSet<MySql.WebApi.Entities.CarImage> CarImage { get; set; } = default!;
	}
}