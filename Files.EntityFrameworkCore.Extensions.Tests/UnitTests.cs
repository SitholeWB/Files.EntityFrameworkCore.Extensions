using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Files.EntityFrameworkCore.Extensions.Tests
{
	public class UnitTests
	{
		private MyDbContext _dbContext;

		[SetUp]
		public void Setup()
		{
			_dbContext = GetDatabaseContext();
		}

		[TestCase(20)]
		[TestCase(30)]
		[TestCase(40)]
		[TestCase(100)]
		public void AddFile_GivenValidInput_And_Invoke_SaveChanges_ShouldSaveChangesToDatabase(int paragraphs)
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(paragraphs);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var response = _dbContext.AddFile<TextStreamEntity>(stream, lorem.Word(), "plain/text", chunkSize: 7);
			_dbContext.SaveChanges();
			Assert.True(_dbContext.TextStreamEntities.Count(x => x.Id == response.Id) > 0);
		}

		[TestCase(20)]
		[TestCase(30)]
		[TestCase(40)]
		[TestCase(100)]
		public void AddFile_GivenValidInput_And_WithoutInvoke_SaveChanges_ShouldSaveChangesToDatabase(int paragraphs)
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(paragraphs);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var response = _dbContext.AddFile<TextStreamEntity>(stream, lorem.Word(), "plain/text", chunkSize: 7);

			Assert.True(_dbContext.TextStreamEntities.Count(x => x.Id == response.Id) == 1);
		}

		[TestCase(20)]
		[TestCase(30)]
		[TestCase(40)]
		[TestCase(100)]
		public void SaveFile_GivenValidInput_And_WithoutInvoke_SaveChanges_ShouldSaveChangesToDatabase(int paragraphs)
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(paragraphs);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var response = _dbContext.SaveFile<TextStreamEntity>(stream, lorem.Word(), "plain/text", chunkSize: 7);

			Assert.True(_dbContext.TextStreamEntities.Count(x => x.Id == response.Id) > 0);
		}

		[TestCase(20)]
		[TestCase(30)]
		[TestCase(40)]
		[TestCase(100)]
		public void SaveFile_GivenValidInput_ShouldHaveFileIdOnAllAddedRows(int paragraphs)
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(paragraphs);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var response = _dbContext.SaveFile<TextStreamEntity>(stream, lorem.Word(), "plain/text", chunkSize: 7);

			Assert.True(_dbContext.TextStreamEntities.Count(x => x.FileId != response.Id) == 0);
		}

		[Test]
		public void AddFile_GivenValidInput_ShouldHaveFileIdOnAllAddedRows()
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(20);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var response = _dbContext.AddFile<TextStreamEntity>(stream, lorem.Word(), "plain/text", chunkSize: 7);
			_dbContext.SaveChanges();
			Assert.True(_dbContext.TextStreamEntities.Count(x => x.FileId != response.Id) == 0);
		}

		[TestCase(20)]
		[TestCase(30)]
		[TestCase(40)]
		[TestCase(100)]
		public void AddFile_GivenValidInput_ShouldSaveMimeType(int paragraphs)
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(paragraphs);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var response = _dbContext.AddFile<TextStreamEntity>(stream, lorem.Word(), "plain/text", chunkSize: 7);
			_dbContext.SaveChanges();
			Assert.True(_dbContext.TextStreamEntities.Count(x => x.MimeType != "plain/text") == 0);
		}

		[TestCase(20)]
		[TestCase(30)]
		[TestCase(40)]
		[TestCase(100)]
		public void SaveFile_GivenValidInput_ShouldSaveMimeType(int paragraphs)
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(paragraphs);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var response = _dbContext.SaveFile<TextStreamEntity>(stream, lorem.Word(), "plain/text", chunkSize: 7);

			Assert.True(_dbContext.TextStreamEntities.Count(x => x.MimeType != "plain/text") == 0);
		}

		[TestCase(20)]
		[TestCase(30)]
		[TestCase(40)]
		[TestCase(100)]
		public void SaveFile_GivenValidInput_ShouldSaveName(int paragraphs)
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(paragraphs);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var filename = lorem.Word();
			var response = _dbContext.SaveFile<TextStreamEntity>(stream, filename, "plain/text", chunkSize: 7);

			Assert.True(_dbContext.TextStreamEntities.Count(x => x.Name != filename) == 0);
		}

		[TestCase(20)]
		[TestCase(30)]
		[TestCase(40)]
		[TestCase(100)]
		public void SaveFile_GivenValidInput_ShouldSaveChunkSize(int paragraphs)
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(paragraphs);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var filename = lorem.Word();
			var response = _dbContext.SaveFile<TextStreamEntity>(stream, filename, "plain/text", chunkSize: 7);

			Assert.True(_dbContext.TextStreamEntities.Count(x => x.ChunkBytesLength == 7) > 0);
		}

		[TestCase(20)]
		[TestCase(30)]
		[TestCase(40)]
		[TestCase(100)]
		public void AddFile_GivenValidInput_ShouldSaveChunkSize(int paragraphs)
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(paragraphs);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var filename = lorem.Word();
			var response = _dbContext.AddFile<TextStreamEntity>(stream, filename, "plain/text", chunkSize: 7);
			_dbContext.SaveChanges();
			Assert.True(_dbContext.TextStreamEntities.Count(x => x.ChunkBytesLength == 7) > 0);
		}

		[TestCase(20)]
		[TestCase(30)]
		[TestCase(40)]
		[TestCase(100)]
		public void SaveFile_GivenValidInput_ShouldSaveGivenId(int paragraphs)
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(paragraphs);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var filename = lorem.Word();
			var id = Guid.NewGuid();
			var response = _dbContext.SaveFile<TextStreamEntity>(stream, filename, "plain/text", chunkSize: 7, fileId: id);

			Assert.True(_dbContext.TextStreamEntities.Count(x => x.FileId == id) > 0);
		}

		[TestCase(20)]
		[TestCase(30)]
		[TestCase(40)]
		[TestCase(100)]
		public void AddFile_GivenValidInput_ShouldSaveGivenId(int paragraphs)
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(paragraphs);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var filename = lorem.Word();
			var id = Guid.NewGuid();
			var response = _dbContext.AddFile<TextStreamEntity>(stream, filename, "plain/text", chunkSize: 7, fileId: id);
			_dbContext.SaveChanges();
			Assert.True(_dbContext.TextStreamEntities.Count(x => x.FileId == id) > 0);
		}

		[TestCase(20)]
		[TestCase(30)]
		[TestCase(40)]
		[TestCase(100)]
		public void AddFile_GivenValidInput_ShouldSaveName(int paragraphs)
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(paragraphs);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var filename = lorem.Word();
			var response = _dbContext.AddFile<TextStreamEntity>(stream, filename, "plain/text", chunkSize: 7);
			_dbContext.SaveChanges();
			Assert.True(_dbContext.TextStreamEntities.Count(x => x.Name != filename) == 0);
		}

		[TestCase(20)]
		[TestCase(30)]
		[TestCase(40)]
		[TestCase(100)]
		[TestCase(500)]
		[TestCase(1000)]
		public void DownloadFileToStream_GivenValidInput_SaveFile_ShouldDownloadMatchingWithInput(int paragraphs)
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(paragraphs);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var filename = lorem.Word();
			var chunkSize = new Random().Next(60) + 10;
			var response = _dbContext.SaveFile<TextStreamEntity>(stream, filename, "plain/text", chunkSize: chunkSize);

			var streamOutput = new MemoryStream();
			_dbContext.DownloadFileToStream<TextStreamEntity>(response.Id, streamOutput);

			var resultString = Encoding.ASCII.GetString(streamOutput.ToArray());

			Assert.AreEqual(inputText, resultString);
		}

		[TestCase(20)]
		[TestCase(30)]
		[TestCase(40)]
		[TestCase(100)]
		[TestCase(500)]
		[TestCase(1000)]
		public void DownloadFileToStream_GivenValidInput_AddFile_ShouldDownloadMatchingWithInput(int paragraphs)
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(paragraphs);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var filename = lorem.Word();
			var chunkSize = new Random().Next(60) + 10;
			var response = _dbContext.AddFile<TextStreamEntity>(stream, filename, "plain/text", chunkSize: chunkSize);
			_dbContext.SaveChanges();
			var streamOutput = new MemoryStream();
			_dbContext.DownloadFileToStream<TextStreamEntity>(response.Id, streamOutput);

			var resultString = Encoding.ASCII.GetString(streamOutput.ToArray());

			Assert.AreEqual(inputText, resultString);
		}

		[TestCase(10)]
		[TestCase(20)]
		[TestCase(30)]
		[TestCase(60)]
		public void DeleteFile_GivenValidInput_SaveFile_ShouldDeleteAllChunksForGivenId(int paragraphs)
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(paragraphs);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var filename = lorem.Word();
			var chunkSize = new Random().Next(60) + 10;
			var response = _dbContext.SaveFile<TextStreamEntity>(stream, filename, "plain/text", chunkSize: chunkSize);

			Assert.True(_dbContext.TextStreamEntities.Count(x => x.FileId == response.Id) > 0);

			_dbContext.DeleteFile<TextStreamEntity>(response.Id);
			_dbContext.SaveChanges();

			Assert.True(_dbContext.TextStreamEntities.Count(x => x.FileId == response.Id) == 0);
		}

		[TestCase(10)]
		[TestCase(20)]
		[TestCase(30)]
		[TestCase(60)]
		public void DeleteFile_GivenValidInput_AddFile_ShouldDeleteAllChunksForGivenId(int paragraphs)
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(paragraphs);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var filename = lorem.Word();
			var chunkSize = new Random().Next(60) + 10;
			var response = _dbContext.AddFile<TextStreamEntity>(stream, filename, "plain/text", chunkSize: chunkSize);
			_dbContext.SaveChanges();
			Assert.True(_dbContext.TextStreamEntities.Count(x => x.FileId == response.Id) > 0);

			_dbContext.DeleteFile<TextStreamEntity>(response.Id);
			_dbContext.SaveChanges();

			Assert.True(_dbContext.TextStreamEntities.Count(x => x.FileId == response.Id) == 0);
		}

		[TestCase(10)]
		[TestCase(20)]
		[TestCase(30)]
		[TestCase(60)]
		public void GetFileInfo_GivenValidInput_SaveFile_ShouldDeleteAllChunksForGivenId(int paragraphs)
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(paragraphs);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var filename = lorem.Word();
			var chunkSize = new Random().Next(60) + 10;
			var response = _dbContext.SaveFile<TextStreamEntity>(stream, filename, "plain/text", chunkSize: chunkSize);

			Assert.True(_dbContext.TextStreamEntities.Count(x => x.FileId == response.Id) > 0);

			var fileInfo = _dbContext.GetFileInfo<TextStreamEntity>(response.Id);

			Assert.AreEqual(response.Name, fileInfo.Name);
			Assert.AreEqual(response.MimeType, fileInfo.MimeType);
			Assert.AreEqual(response.TotalBytesLength, stream.Length);
			Assert.Greater(fileInfo.TimeStamp, DateTimeOffset.Now.AddDays(-1));
		}

		[TestCase(10)]
		[TestCase(20)]
		[TestCase(30)]
		[TestCase(60)]
		public void GetFileInfo_GivenValidInput_AddFile_ShouldDeleteAllChunksForGivenId(int paragraphs)
		{
			var lorem = new Bogus.DataSets.Lorem(locale: "zu_ZA");
			var inputText = lorem.Paragraphs(paragraphs);

			var bytes = Encoding.ASCII.GetBytes(inputText);

			var stream = new MemoryStream(bytes);
			var filename = lorem.Word();
			var chunkSize = new Random().Next(60) + 10;
			var response = _dbContext.AddFile<TextStreamEntity>(stream, filename, "plain/text", chunkSize: chunkSize);
			_dbContext.SaveChanges();
			Assert.True(_dbContext.TextStreamEntities.Count(x => x.FileId == response.Id) > 0);

			var fileInfo = _dbContext.GetFileInfo<TextStreamEntity>(response.Id);

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