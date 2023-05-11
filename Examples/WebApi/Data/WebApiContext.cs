using Microsoft.EntityFrameworkCore;
using WebApi.Entities;

namespace WebApi.Data
{
	public class WebApiContext : DbContext
	{
		public WebApiContext(DbContextOptions<WebApiContext> options)
			: base(options)
		{
			Database.Migrate();
		}

		public DbSet<UserImage> UserImage { get; set; }
	}
}