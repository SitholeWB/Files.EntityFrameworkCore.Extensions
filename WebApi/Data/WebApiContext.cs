using Microsoft.EntityFrameworkCore;
using WebApi.Entities;

namespace WebApi.Data
{
	public class WebApiContext : DbContext
	{
		public WebApiContext(DbContextOptions<WebApiContext> options)
			: base(options)
		{
		}

		public DbSet<User> User { get; set; } = default!;

		public DbSet<WebApi.Entities.UserImage>? UserImage { get; set; }
	}
}