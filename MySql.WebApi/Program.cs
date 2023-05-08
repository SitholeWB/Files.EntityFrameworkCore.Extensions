using Microsoft.EntityFrameworkCore;
using MySql.WebApi.Data;

namespace MySql.WebApi
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);
			var serverVersion = new MySqlServerVersion(new Version(8, 0, 31));
			builder.Services.AddDbContext<MySqlWebApiContext>(options =>
				options.UseMySql(builder.Configuration.GetConnectionString("MySqlWebApiContext"), serverVersion));

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