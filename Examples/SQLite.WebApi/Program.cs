using Microsoft.EntityFrameworkCore;
using SQLite.WebApi.Data;

namespace SQLite.WebApi
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);
			builder.Services.AddDbContext<SQLiteWebApiContext>(options =>
				options.UseSqlite(builder.Configuration.GetConnectionString("SQLiteWebApiContext") ?? throw new InvalidOperationException("Connection string 'SQLiteWebApiContext' not found.")));

			// Add services to the container.

			builder.Services.AddControllers();
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseHttpsRedirection();

			app.UseAuthorization();

			app.MapControllers();

			app.Run();
		}
	}
}