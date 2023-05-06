using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Files.EntityFrameworkCore.Extensions.Tests
{
	public class UnitTests
	{
		private MyDbContext _dbContext;

		[SetUp]
		public async Task Setup()
		{
			_dbContext = GetDatabaseContext();
		}

		[Test]
		public async Task AddFileAsync_GivenValidInput_And_Invoke_SaveChangesAsync_ShouldSaveChangesToDatabase()
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(20);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var response = await _dbContext.AddFileAsync<TextStreamEntity>(stream, lorem.Word(), "plain/text", chunkSize: 7);
			await _dbContext.SaveChangesAsync();

			Assert.True(_dbContext.TextStreamEntities.Count(x => x.Id == response) > 0);
		}

		[Test]
		public async Task AddFileAsync_GivenValidInput_And_WithoutInvoke_SaveChangesAsync_ShouldNotSaveChangesToDatabase()
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(20);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var response = await _dbContext.AddFileAsync<TextStreamEntity>(stream, lorem.Word(), "plain/text", chunkSize: 7);

			Assert.True(_dbContext.TextStreamEntities.Count(x => x.Id == response) == 0);
		}

		[Test]
		public async Task SaveFileAsync_GivenValidInput_And_WithoutInvoke_SaveChangesAsync_ShouldSaveChangesToDatabase()
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(20);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var response = await _dbContext.SaveFileAsync<TextStreamEntity>(stream, lorem.Word(), "plain/text", chunkSize: 7);

			Assert.True(_dbContext.TextStreamEntities.Count(x => x.Id == response) > 0);
		}

		[Test]
		public async Task SaveFileAsync_GivenValidInput_ShouldHaveFileIdOnAllAddedRows()
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(20);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var response = await _dbContext.SaveFileAsync<TextStreamEntity>(stream, lorem.Word(), "plain/text", chunkSize: 7);

			Assert.True(_dbContext.TextStreamEntities.Count(x => x.FileId != response) == 0);
		}

		[Test]
		public async Task AddFileAsync_GivenValidInput_ShouldHaveFileIdOnAllAddedRows()
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(20);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var response = await _dbContext.AddFileAsync<TextStreamEntity>(stream, lorem.Word(), "plain/text", chunkSize: 7);

			Assert.True(_dbContext.TextStreamEntities.Count(x => x.FileId != response) == 0);
		}

		private MyDbContext GetDatabaseContext()
		{
			var options = new DbContextOptionsBuilder<MyDbContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;
			var databaseContext = new MyDbContext(options);
			databaseContext.Database.EnsureCreated();
			return databaseContext;
		}
	}
}