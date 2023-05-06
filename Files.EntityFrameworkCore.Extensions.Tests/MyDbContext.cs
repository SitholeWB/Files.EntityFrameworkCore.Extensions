using Microsoft.EntityFrameworkCore;

namespace Files.EntityFrameworkCore.Extensions.Tests
{
	public class MyDbContext : DbContext
	{
		public MyDbContext(DbContextOptions<MyDbContext> options)
			: base(options)
		{
		}

		public DbSet<TextStreamEntity> TextStreamEntities { get; set; } = default!;

		public DbSet<TextStreamExtendedEntity> TextStreamExtendedEntities { get; set; }
	}
}