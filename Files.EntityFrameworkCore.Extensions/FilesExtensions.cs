using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace Files.EntityFrameworkCore.Extensions
{
	public static class FilesExtensions
	{
		public static IFileEntity GetFile<T>(this DbContext dbContext, Guid id) where T : class, IFileEntity
		{
			return dbContext.Set<T>().Find(id);
		}

		public static Guid SaveFile<T>(this DbContext dbContext, Stream stream, string name, string mimeType = "application/octet-stream", Guid? fileId = null, int? chunkSize = null) where T : class, IFileEntity, new()
		{
			var bufferLen = (chunkSize.HasValue && chunkSize.Value > 0) ? chunkSize.Value : FileHelper.DEFAULT_MAX_CHUNK_SIZE;
			var buffer = new byte[bufferLen];
			var nextId = Guid.NewGuid();
			if (!fileId.HasValue || fileId.Value.Equals(Guid.Empty))
			{
				fileId = Guid.NewGuid();
			}

			var isFirstSave = true;

			long bytesRead;
			do
			{
				var tempT = new T
				{
					Id = nextId,
					FileId = fileId.Value,
					Name = name,
					ChunkBytesLength = bufferLen,
					MimeType = string.IsNullOrWhiteSpace(mimeType) ? "application/octet-stream" : mimeType,
					TimeStamp = DateTimeOffset.UtcNow,
					TotalBytesLength = stream.Length
				};

				nextId = Guid.NewGuid();

				bytesRead = stream.Read(buffer, 0, buffer.Length);
				tempT.Data = buffer;

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