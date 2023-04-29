using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Files.EntityFrameworkCore.Extensions
{
	public static class AsyncFilesExtensions
	{
		public static async Task<IFileEntity> GetFileAsync<T>(this DbContext dbContext, Guid id) where T : class, IFileEntity
		{
			return await dbContext.Set<T>().FindAsync(id);
		}

		public static async Task<FilesExtensionsResponse> GetFileStreamAsync<T>(this DbContext dbContext, Guid id) where T : class, IFileEntity
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

			await response.Stream.WriteAsync(mainFile.Data, 0, mainFile.ChunkBytesLength);
			var nextId = mainFile.NextId;
			while (mainFile.NextId.HasValue)
			{
				mainFile = dbContext.Set<T>().Find(nextId);
				await response.Stream.WriteAsync(mainFile.Data, 0, mainFile.ChunkBytesLength);
				nextId = mainFile.NextId;
			}
			response.Stream.Position = 0;
			return response;
		}

		public static async Task<Guid> SaveFileAsync<T>(this DbContext dbContext, Stream stream, string name, string mimeType = "application/octet-stream", Guid? fileId = null, int? chunkSize = null) where T : class, IFileEntity, new()
		{
			var bufferLen = (chunkSize.HasValue && chunkSize.Value > 0) ? chunkSize.Value : FileHelper.MAX_CHUNK_SIZE;
			var initialBufferLen = bufferLen;
			var buffer = new byte[bufferLen];
			var nextId = Guid.NewGuid();
			if (!fileId.HasValue || fileId.Value.Equals(Guid.Empty))
			{
				fileId = Guid.NewGuid();
			}

			var isFirstSave = true;
			var bytesRead = 0;
			long bytesCount = 0;
			using var SHA_256 = SHA512.Create();

			do
			{
				if ((bytesCount + bufferLen) > stream.Length)
				{
					bufferLen = (int)(stream.Length - bytesCount);
					buffer = new byte[bufferLen];
				}

				if (bufferLen <= 0)
				{
					break;
				}

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

				bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
				tempT.Data = buffer;
				tempT.Hash = BitConverter.ToString(SHA_256.ComputeHash(buffer)).Replace("-", "");

				if (isFirstSave)
				{
					tempT.Id = fileId.Value;
					isFirstSave = false;
				}
				if (bytesRead == initialBufferLen)
				{
					tempT.NextId = nextId;
				}
				bytesCount += bufferLen;
				dbContext.Add(tempT);
			} while (true);

			return fileId.Value;
		}
	}
}