using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Files.EntityFrameworkCore.Extensions
{
	public static class AsyncFilesExtensions
	{
		public static async Task<IFileEntity> GetFileInfoAsync<T>(this DbContext dbContext, Guid id, CancellationToken cancellationToken = default) where T : class, IFileEntity
		{
			return await dbContext.Set<T>().FirstOrDefaultAsync<T>(x => x.Id == id, cancellationToken);
		}

		/*
		public static async Task<FilesExtensionsResponse> GetFileStreamAsync<T>(this DbContext dbContext, Guid id) where T : class, IFileEntity
		{
			var response = new FilesExtensionsResponse { };
			var mainFile = await dbContext.Set<T>().FirstOrDefaultAsync<T>(x => x.Id == id);
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
				mainFile = await dbContext.Set<T>().FirstOrDefaultAsync<T>(x => x.Id == nextId);
				if (mainFile == null)
				{
					//Should never be null but someone might mess up with chunks in the database
					break;
				}
				await response.Stream.WriteAsync(mainFile.Data, 0, mainFile.ChunkBytesLength);
				nextId = mainFile.NextId;
			}
			return response;
		}
		*/

		public static async Task DownloadFileToStreamAsync<T>(this DbContext dbContext, Guid id, Stream outputStream, CancellationToken cancellationToken = default) where T : class, IFileEntity
		{
			var mainFile = await dbContext.Set<T>().FirstOrDefaultAsync<T>(x => x.Id == id, cancellationToken);
			if (mainFile == null)
			{
				throw new FileNotFoundException();
			}
			await outputStream.WriteAsync(mainFile.Data, 0, mainFile.ChunkBytesLength, cancellationToken);
			var nextId = mainFile.NextId;
			while (mainFile.NextId.HasValue)
			{
				mainFile = await dbContext.Set<T>().FirstOrDefaultAsync<T>(x => x.Id == nextId, cancellationToken);
				if (mainFile == null)
				{
					//Should never be null but someone might mess up with chunks in the database
					break;
				}
				await outputStream.WriteAsync(mainFile.Data, 0, mainFile.ChunkBytesLength, cancellationToken);
				nextId = mainFile.NextId;
			}
			try
			{
				outputStream.Position = 0;
			}
			catch
			{
				//Other streams may block the setting of this, so don't break. Allow caller to handle position
			}
		}

		public static async Task DeleteFileAsync<T>(this DbContext dbContext, Guid id, CancellationToken cancellationToken = default) where T : class, IFileEntity
		{
			var mainFile = await dbContext.Set<T>().FirstOrDefaultAsync<T>(x => x.Id == id, cancellationToken);
			if (mainFile == null)
			{
				throw new FileNotFoundException();
			}

			dbContext.Remove(mainFile);
			var nextId = mainFile.NextId;
			while (mainFile.NextId.HasValue)
			{
				mainFile = await dbContext.Set<T>().FirstOrDefaultAsync<T>(x => x.Id == nextId, cancellationToken);
				if (mainFile == null)
				{
					//Should never be null but someone might mess up with chunks in the database
					break;
				}
				dbContext.Remove(mainFile);
				nextId = mainFile.NextId;
			}
		}

		private static async Task<Guid> AddOrSaveFileHelperAsync<T>(DbContext dbContext, Stream stream, string name, string mimeType = "application/octet-stream", Guid? fileId = null, int? chunkSize = null, bool eagerSave = false, CancellationToken cancellationToken = default) where T : class, IFileEntity, new()
		{
			var bufferLen = (chunkSize.HasValue && chunkSize.Value > 0) ? chunkSize.Value : FileHelper.DEFAULT_MAX_CHUNK_SIZE;
			var initialBufferLen = bufferLen;
			var nextId = Guid.NewGuid();
			if (!fileId.HasValue || fileId.Value.Equals(Guid.Empty))
			{
				fileId = Guid.NewGuid();
			}

			var isFirstSave = true;
			do
			{
				var buffer = new byte[bufferLen];
				if ((stream.Length - stream.Position) <= bufferLen)
				{
					bufferLen = (int)(stream.Length - stream.Position);
					if (bufferLen <= 0)
					{
						break;
					}
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
					MimeType = string.IsNullOrWhiteSpace(mimeType) ? "application/octet-stream" : mimeType,
					TimeStamp = DateTimeOffset.UtcNow,
					TotalBytesLength = stream.Length
				};

				nextId = Guid.NewGuid();

				var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
				tempT.Data = buffer;

				if (isFirstSave)
				{
					tempT.Id = fileId.Value;
					isFirstSave = false;
				}
				if (bytesRead == initialBufferLen)
				{
					tempT.NextId = nextId;
				}
				dbContext.Add(tempT);
				if (eagerSave)
				{
					await dbContext.SaveChangesAsync(cancellationToken);
				}
			} while (true);

			return fileId.Value;
		}

		/// <summary>
		/// The DbContext method "SaveChangesAsync()" will NOT be called after every chunk, this
		/// will cause Entity Framework to keep so many large chunks in-memory. 
		/// You must explicitly call the SaveChangesAsync()
		/// NOTE: Default chunk size = 64k
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dbContext"></param>
		/// <param name="stream"></param>
		/// <param name="name"></param>
		/// <param name="mimeType"></param>
		/// <param name="fileId"></param>
		/// <param name="chunkSize"></param>
		/// <returns></returns>
		public static async Task<Guid> AddFileAsync<T>(this DbContext dbContext, Stream stream, string name, string mimeType = "application/octet-stream", Guid? fileId = null, int? chunkSize = null, CancellationToken cancellationToken = default) where T : class, IFileEntity, new()
		{
			return await AddOrSaveFileHelperAsync<T>(dbContext, stream, name, mimeType, fileId, chunkSize, false, cancellationToken);
		}

		/// <summary>
		/// The DbContext method "SaveChangesAsync()" will be called after every chunk, this is
		/// important if you want to avoid Entity Framework keeping so many large chunks in-memory.
		/// NOTE: Default chunk size = 64k
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dbContext"></param>
		/// <param name="stream"></param>
		/// <param name="name"></param>
		/// <param name="mimeType"></param>
		/// <param name="fileId"></param>
		/// <param name="chunkSize"></param>
		/// <returns></returns>
		public static async Task<Guid> SaveFileAsync<T>(this DbContext dbContext, Stream stream, string name, string mimeType = "application/octet-stream", Guid? fileId = null, int? chunkSize = null, CancellationToken cancellationToken = default) where T : class, IFileEntity, new()
		{
			return await AddOrSaveFileHelperAsync<T>(dbContext, stream, name, mimeType, fileId, chunkSize, true, cancellationToken);
		}
	}
}