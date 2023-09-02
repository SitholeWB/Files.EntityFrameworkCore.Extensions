using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Files.EntityFrameworkCore.Extensions.Tests
{
	public class AsyncUnitTests
	{
		private MyDbContext _dbContext;

		[SetUp]
		public async Task Setup()
		{
			_dbContext = GetDatabaseContext();
		}

		[TestCase(20)]
		[TestCase(30)]
		[TestCase(40)]
		[TestCase(100)]
		public async Task AddFileAsync_GivenValidInput_And_Invoke_SaveChangesAsync_ShouldSaveChangesToDatabase(int paragraphs)
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(paragraphs);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var response = await _dbContext.AddFileAsync<TextStreamEntity>(stream, lorem.Word(), "plain/text", chunkSize: 7);
			await _dbContext.SaveChangesAsync();

			Assert.True(_dbContext.TextStreamEntities.Count(x => x.Id == response.Id) > 0);
		}

		[TestCase(20)]
		[TestCase(30)]
		[TestCase(40)]
		[TestCase(100)]
		public async Task AddFileAsync_GivenValidInput_And_WithoutInvoke_SaveChangesAsync_ShouldSaveChangesToDatabase(int paragraphs)
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(paragraphs);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var response = await _dbContext.AddFileAsync<TextStreamEntity>(stream, lorem.Word(), "plain/text", chunkSize: 7);

			Assert.True(_dbContext.TextStreamEntities.Count(x => x.Id == response.Id) == 1);
		}

		[TestCase(20)]
		[TestCase(30)]
		[TestCase(40)]
		[TestCase(100)]
		public async Task SaveFileAsync_GivenValidInput_And_WithoutInvoke_SaveChangesAsync_ShouldSaveChangesToDatabase(int paragraphs)
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(paragraphs);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var response = await _dbContext.SaveFileAsync<TextStreamEntity>(stream, lorem.Word(), "plain/text", chunkSize: 7);

			Assert.True(_dbContext.TextStreamEntities.Count(x => x.Id == response.Id) > 0);
		}

		[TestCase(20)]
		[TestCase(30)]
		[TestCase(40)]
		[TestCase(100)]
		public async Task SaveFileAsync_GivenValidInput_ShouldHaveFileIdOnAllAddedRows(int paragraphs)
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(paragraphs);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var response = await _dbContext.SaveFileAsync<TextStreamEntity>(stream, lorem.Word(), "plain/text", chunkSize: 7);

			Assert.True(_dbContext.TextStreamEntities.Count(x => x.FileId != response.Id) == 0);
		}

		[Test]
		public async Task AddFileAsync_GivenValidInput_ShouldHaveFileIdOnAllAddedRows()
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(20);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var response = await _dbContext.AddFileAsync<TextStreamEntity>(stream, lorem.Word(), "plain/text", chunkSize: 7);
			await _dbContext.SaveChangesAsync();
			Assert.True(_dbContext.TextStreamEntities.Count(x => x.FileId != response.Id) == 0);
		}

		[TestCase(20)]
		[TestCase(30)]
		[TestCase(40)]
		[TestCase(100)]
		public async Task AddFileAsync_GivenValidInput_ShouldSaveMimeType(int paragraphs)
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(paragraphs);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var response = await _dbContext.AddFileAsync<TextStreamEntity>(stream, lorem.Word(), "plain/text", chunkSize: 7);
			await _dbContext.SaveChangesAsync();
			Assert.True(_dbContext.TextStreamEntities.Count(x => x.MimeType != "plain/text") == 0);
		}

		[TestCase(20)]
		[TestCase(30)]
		[TestCase(40)]
		[TestCase(100)]
		public async Task SaveFileAsync_GivenValidInput_ShouldSaveMimeType(int paragraphs)
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(paragraphs);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var response = await _dbContext.SaveFileAsync<TextStreamEntity>(stream, lorem.Word(), "plain/text", chunkSize: 7);

			Assert.True(_dbContext.TextStreamEntities.Count(x => x.MimeType != "plain/text") == 0);
		}

		[TestCase(20)]
		[TestCase(30)]
		[TestCase(40)]
		[TestCase(100)]
		public async Task SaveFileAsync_GivenValidInput_ShouldSaveName(int paragraphs)
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(paragraphs);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var filename = lorem.Word();
			var response = await _dbContext.SaveFileAsync<TextStreamEntity>(stream, filename, "plain/text", chunkSize: 7);

			Assert.True(_dbContext.TextStreamEntities.Count(x => x.Name != filename) == 0);
		}

		[TestCase(20)]
		[TestCase(30)]
		[TestCase(40)]
		[TestCase(100)]
		public async Task SaveFileAsync_GivenValidInput_ShouldSaveChunkSize(int paragraphs)
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(paragraphs);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var filename = lorem.Word();
			var response = await _dbContext.SaveFileAsync<TextStreamEntity>(stream, filename, "plain/text", chunkSize: 7);

			Assert.True(_dbContext.TextStreamEntities.Count(x => x.ChunkBytesLength == 7) > 0);
		}

		[TestCase(20)]
		[TestCase(30)]
		[TestCase(40)]
		[TestCase(100)]
		public async Task AddFileAsync_GivenValidInput_ShouldSaveChunkSize(int paragraphs)
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(paragraphs);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var filename = lorem.Word();
			var response = await _dbContext.AddFileAsync<TextStreamEntity>(stream, filename, "plain/text", chunkSize: 7);
			await _dbContext.SaveChangesAsync();
			Assert.True(_dbContext.TextStreamEntities.Count(x => x.ChunkBytesLength == 7) > 0);
		}

		[TestCase(20)]
		[TestCase(30)]
		[TestCase(40)]
		[TestCase(100)]
		public async Task SaveFileAsync_GivenValidInput_ShouldSaveGivenId(int paragraphs)
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(paragraphs);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var filename = lorem.Word();
			var id = Guid.NewGuid();
			var response = await _dbContext.SaveFileAsync<TextStreamEntity>(stream, filename, "plain/text", chunkSize: 7, fileId: id);

			Assert.True(_dbContext.TextStreamEntities.Count(x => x.FileId == id) > 0);
		}

		[TestCase(20)]
		[TestCase(30)]
		[TestCase(40)]
		[TestCase(100)]
		public async Task AddFileAsync_GivenValidInput_ShouldSaveGivenId(int paragraphs)
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(paragraphs);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var filename = lorem.Word();
			var id = Guid.NewGuid();
			var response = await _dbContext.AddFileAsync<TextStreamEntity>(stream, filename, "plain/text", chunkSize: 7, fileId: id);
			await _dbContext.SaveChangesAsync();
			Assert.True(_dbContext.TextStreamEntities.Count(x => x.FileId == id) > 0);
		}

		[TestCase(20)]
		[TestCase(30)]
		[TestCase(40)]
		[TestCase(100)]
		public async Task AddFileAsync_GivenValidInput_ShouldSaveName(int paragraphs)
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(paragraphs);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var filename = lorem.Word();
			var response = await _dbContext.AddFileAsync<TextStreamEntity>(stream, filename, "plain/text", chunkSize: 7);
			await _dbContext.SaveChangesAsync();
			Assert.True(_dbContext.TextStreamEntities.Count(x => x.Name != filename) == 0);
		}

		[TestCase(20)]
		[TestCase(30)]
		[TestCase(40)]
		[TestCase(100)]
		[TestCase(500)]
		[TestCase(1000)]
		public async Task DownloadFileToStreamAsync_GivenValidInput_SaveFileAsync_ShouldDownloadMatchingWithInput(int paragraphs)
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(paragraphs);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var filename = lorem.Word();
			var chunkSize = new Random().Next(60) + 10;
			var response = await _dbContext.SaveFileAsync<TextStreamEntity>(stream, filename, "plain/text", chunkSize: chunkSize);

			var streamOutput = new MemoryStream();
			await _dbContext.DownloadFileToStreamAsync<TextStreamEntity>(response.Id, streamOutput);

			var resultString = Encoding.ASCII.GetString(streamOutput.ToArray());

			Assert.AreEqual(inputText, resultString);
		}

		[TestCase(20)]
		[TestCase(30)]
		[TestCase(40)]
		[TestCase(100)]
		[TestCase(500)]
		[TestCase(1000)]
		public async Task DownloadFileToStreamAsync_GivenValidInput_AddFileAsync_ShouldDownloadMatchingWithInput(int paragraphs)
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(paragraphs);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var filename = lorem.Word();
			var chunkSize = new Random().Next(60) + 10;
			var response = await _dbContext.AddFileAsync<TextStreamEntity>(stream, filename, "plain/text", chunkSize: chunkSize);
			await _dbContext.SaveChangesAsync();
			var streamOutput = new MemoryStream();
			await _dbContext.DownloadFileToStreamAsync<TextStreamEntity>(response.Id, streamOutput);

			var resultString = Encoding.ASCII.GetString(streamOutput.ToArray());

			Assert.AreEqual(inputText, resultString);
		}

		[TestCase(10)]
		[TestCase(20)]
		[TestCase(30)]
		[TestCase(60)]
		public async Task DeleteFileAsync_GivenValidInput_SaveFileAsync_ShouldDeleteAllChunksForGivenId(int paragraphs)
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(paragraphs);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var filename = lorem.Word();
			var chunkSize = new Random().Next(60) + 10;
			var response = await _dbContext.SaveFileAsync<TextStreamEntity>(stream, filename, "plain/text", chunkSize: chunkSize);

			Assert.True(_dbContext.TextStreamEntities.Count(x => x.FileId == response.Id) > 0);

			await _dbContext.DeleteFileAsync<TextStreamEntity>(response.Id);
			await _dbContext.SaveChangesAsync();

			Assert.True(_dbContext.TextStreamEntities.Count(x => x.FileId == response.Id) == 0);
		}

		[TestCase(10)]
		[TestCase(20)]
		[TestCase(30)]
		[TestCase(60)]
		public async Task DeleteFileAsync_GivenValidInput_AddFileAsync_ShouldDeleteAllChunksForGivenId(int paragraphs)
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(paragraphs);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var filename = lorem.Word();
			var chunkSize = new Random().Next(60) + 10;
			var response = await _dbContext.AddFileAsync<TextStreamEntity>(stream, filename, "plain/text", chunkSize: chunkSize);
			await _dbContext.SaveChangesAsync();
			Assert.True(_dbContext.TextStreamEntities.Count(x => x.FileId == response.Id) > 0);

			await _dbContext.DeleteFileAsync<TextStreamEntity>(response.Id);
			await _dbContext.SaveChangesAsync();

			Assert.True(_dbContext.TextStreamEntities.Count(x => x.FileId == response.Id) == 0);
		}

		[TestCase(10)]
		[TestCase(20)]
		[TestCase(30)]
		[TestCase(60)]
		public async Task GetFileInfoAsync_GivenValidInput_SaveFileAsync_ShouldDeleteAllChunksForGivenId(int paragraphs)
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(paragraphs);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var filename = lorem.Word();
			var chunkSize = new Random().Next(60) + 10;
			var response = await _dbContext.SaveFileAsync<TextStreamEntity>(stream, filename, "plain/text", chunkSize: chunkSize);

			Assert.True(_dbContext.TextStreamEntities.Count(x => x.FileId == response.Id) > 0);

			var fileInfo = await _dbContext.GetFileInfoAsync<TextStreamEntity>(response.Id);

			Assert.AreEqual(response.Name, fileInfo.Name);
			Assert.AreEqual(response.MimeType, fileInfo.MimeType);
			Assert.AreEqual(response.TotalBytesLength, stream.Length);
			Assert.Greater(fileInfo.TimeStamp, DateTimeOffset.Now.AddDays(-1));
		}

		[TestCase(10)]
		[TestCase(20)]
		[TestCase(30)]
		[TestCase(60)]
		public async Task GetFileInfoAsync_GivenValidInput_AddFileAsync_ShouldDeleteAllChunksForGivenId(int paragraphs)
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(paragraphs);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var filename = lorem.Word();
			var chunkSize = new Random().Next(60) + 10;
			var response = await _dbContext.AddFileAsync<TextStreamEntity>(stream, filename, "plain/text", chunkSize: chunkSize);
			await _dbContext.SaveChangesAsync();
			Assert.True(_dbContext.TextStreamEntities.Count(x => x.FileId == response.Id) > 0);

			var fileInfo = await _dbContext.GetFileInfoAsync<TextStreamEntity>(response.Id);

			Assert.AreEqual(response.Name, fileInfo.Name);
			Assert.AreEqual(response.MimeType, fileInfo.MimeType);
			Assert.AreEqual(response.TotalBytesLength, stream.Length);
			Assert.Greater(fileInfo.TimeStamp, DateTimeOffset.Now.AddDays(-1));
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