using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;

namespace Files.EntityFrameworkCore.Extensions
{
	public static class FilesExtensions
	{
		public static IFileEntity GetFileInfo<T>(this DbContext dbContext, Guid id) where T : class, IFileEntity
		{
			return dbContext.Set<T>().FirstOrDefault<T>(x => x.Id == id);
		}

		public static void DownloadFileToStream<T>(this DbContext dbContext, Guid id, Stream outputStream) where T : class, IFileEntity
		{
			var mainFile = dbContext.Set<T>().FirstOrDefault<T>(x => x.Id == id);
			if (mainFile == null)
			{
				throw new FileNotFoundException();
			}
			outputStream.Write(mainFile.Data, 0, mainFile.ChunkBytesLength);
			var nextId = mainFile.NextId;
			while (mainFile.NextId.HasValue)
			{
				mainFile = dbContext.Set<T>().FirstOrDefault<T>(x => x.Id == nextId);
				if (mainFile == null)
				{
					//Should never be null but someone might mess up with chunks in the database
					break;
				}
				outputStream.Write(mainFile.Data, 0, mainFile.ChunkBytesLength);
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

		public static void DeleteFile<T>(this DbContext dbContext, Guid id) where T : class, IFileEntity
		{
			var mainFile = dbContext.Set<T>().FirstOrDefault<T>(x => x.Id == id);
			if (mainFile == null)
			{
				throw new FileNotFoundException();
			}

			dbContext.Remove(mainFile);
			var nextId = mainFile.NextId;
			while (mainFile.NextId.HasValue)
			{
				mainFile = dbContext.Set<T>().FirstOrDefault<T>(x => x.Id == nextId);
				if (mainFile == null)
				{
					//Should never be null but someone might mess up with chunks in the database
					break;
				}
				dbContext.Remove(mainFile);
				nextId = mainFile.NextId;
			}
		}

		private static Guid AddOrSaveFileHelper<T>(DbContext dbContext, Stream stream, string name, string mimeType = "application/octet-stream", Guid? fileId = null, int? chunkSize = null, bool eagerSave = false) where T : class, IFileEntity, new()
		{
			var bufferLen = (chunkSize.HasValue && chunkSize.Value > 0) ? chunkSize.Value : FileHelper.DEFAULT_MAX_CHUNK_SIZE;
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

				stream.Read(buffer, 0, buffer.Length);
				tempT.Data = buffer;

				if (isFirstSave)
				{
					tempT.Id = fileId.Value;
					isFirstSave = false;
				}
				if (stream.Length != stream.Position)
				{
					tempT.NextId = nextId;
				}
				dbContext.Add(tempT);
				if (eagerSave)
				{
					dbContext.SaveChanges();
				}
			} while (stream.Length != stream.Position);

			return fileId.Value;
		}

		/// <summary>
		/// The DbContext method "SaveChanges()" will NOT be called after every chunk, this will
		/// cause Entity Framework to keep so many large chunks in-memory. You must explicitly call
		/// the SaveChanges() method
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
		public static Guid AddFile<T>(this DbContext dbContext, Stream stream, string name, string mimeType = "application/octet-stream", Guid? fileId = null, int? chunkSize = null) where T : class, IFileEntity, new()
		{
			return AddOrSaveFileHelper<T>(dbContext, stream, name, mimeType, fileId, chunkSize, false);
		}

		/// <summary>
		/// The DbContext method "SaveChanges()" will be called after every chunk, this is important
		/// if you want to avoid Entity Framework keeping so many large chunks in-memory.
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
		public static Guid SaveFile<T>(this DbContext dbContext, Stream stream, string name, string mimeType = "application/octet-stream", Guid? fileId = null, int? chunkSize = null) where T : class, IFileEntity, new()
		{
			return AddOrSaveFileHelper<T>(dbContext, stream, name, mimeType, fileId, chunkSize, true);
		}
	}
}