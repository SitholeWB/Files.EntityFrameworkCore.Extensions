using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Security.Cryptography;

namespace Files.EntityFrameworkCore.Extensions
{
	public static class FilesExtensions
	{
		public static IFileEntity GetFile<T>(this DbContext dbContext, Guid id) where T : class, IFileEntity
		{
			return dbContext.Set<T>().Find(id);
		}

		public static FilesExtensionsResponse GetFileStreamAsync<T>(this DbContext dbContext, Guid id) where T : class, IFileEntity
		{
			var response = new FilesExtensionsResponse { };
			var mainFile = dbContext.Set<T>().Find(id);
			if (mainFile == null)
			{
				return response;
			}
			response.Length = mainFile.TotalBytesLength;
			response.Name = mainFile.Name;
			response.MimeType = mainFile.MimeType;
			response.Stream = new MemoryStream();
			while (mainFile.NextId.HasValue)
			{
				mainFile = dbContext.Set<T>().Find(mainFile.NextId.Value);
			}

			return response;
		}

		public static Guid SaveFile<T>(this DbContext dbContext, Stream stream, string name, string mimeType = "application/octet-stream", Guid? fileId = null, long? chunkSize = null) where T : class, IFileEntity, new()
		{
			var bufferLen = (chunkSize.HasValue && chunkSize.Value > 0) ? chunkSize.Value : FileHelper.MAX_CHUNK_SIZE;
			var buffer = new byte[bufferLen];
			var nextId = Guid.NewGuid();
			if (!fileId.HasValue || fileId.Value.Equals(Guid.Empty))
			{
				fileId = Guid.NewGuid();
			}

			var isFirstSave = true;
			using var SHA_256 = SHA512.Create();
			long bytesRead;
			do
			{
				var tempT = new T
				{
					Id = nextId,
					FileId = fileId.Value,
					Name = name,
					ChunkBytesLength = bufferLen,
					Start = stream.Position,
					MimeType = string.IsNullOrWhiteSpace(mimeType) ? "application/octet-stream" : mimeType,
					TimeStamp = DateTimeOffset.UtcNow,
					TotalBytesLength = stream.Length
				};

				nextId = Guid.NewGuid();

				bytesRead = stream.Read(buffer, 0, buffer.Length);
				tempT.Data = buffer;
				tempT.Hash = BitConverter.ToString(SHA_256.ComputeHash(buffer)).Replace("-", "");
				SHA_256.Clear();

				if (isFirstSave)
				{
					tempT.Id = fileId.Value;
					isFirstSave = false;
				}
				if (bytesRead != 0)
				{
					tempT.NextId = nextId;
				}
				dbContext.Add<T>(tempT);
			} while (bytesRead != 0);

			return fileId.Value;
		}
	}
}