﻿using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;

namespace Files.EntityFrameworkCore.Extensions
{
	/// <summary>
	/// Files Extensions
	/// </summary>
	public static class FilesExtensions
	{
		/// <summary>
		/// Get the file info without bytes array
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dbContext"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		public static FilesExtensionsResponse GetFileInfo<T>(this DbContext dbContext, Guid id) where T : class, IFileEntity
		{
			return dbContext.Set<T>().Select(x => new FilesExtensionsResponse
			{
				Name = x.Name,
				Id = x.Id,
				MimeType = x.MimeType,
				TimeStamp = x.TimeStamp,
				TotalBytesLength = x.TotalBytesLength,
			}).FirstOrDefault(x => x.Id == id);
		}

		/// <summary>
		/// Download the file to given stream
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dbContext"></param>
		/// <param name="id">File Id</param>
		/// <param name="outputStream">Stream to receive file</param>
		/// <returns></returns>
		/// <exception cref="FileNotFoundException"></exception>
		public static void DownloadFileToStream<T>(this DbContext dbContext, Guid id, Stream outputStream) where T : class, IFileEntity
		{
			var mainFile = dbContext.Set<T>().AsNoTracking().FirstOrDefault<T>(x => x.Id == id);
			if (mainFile == null)
			{
				throw new FileNotFoundException();
			}
			outputStream.Write(mainFile.Data, 0, mainFile.ChunkBytesLength);
			var nextId = mainFile.NextId;
			while (mainFile.NextId.HasValue)
			{
				mainFile = dbContext.Set<T>().AsNoTracking().FirstOrDefault<T>(x => x.Id == nextId);
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

		/// <summary>
		/// Delete the file for given id, all chunks matching the id will be deleted.
		/// NOTE: SaveChanges is auto called on every chunk so that memory usage remain low.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dbContext"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		/// <exception cref="FileNotFoundException"></exception>
		public static void DeleteFile<T>(this DbContext dbContext, Guid id) where T : class, IFileEntity
		{
			var mainFile = dbContext.Set<T>().FirstOrDefault<T>(x => x.Id == id);
			if (mainFile == null)
			{
				throw new FileNotFoundException();
			}

			dbContext.Remove(mainFile);
			dbContext.SaveChanges();
			dbContext.Entry<T>(mainFile).State = EntityState.Detached;
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
				dbContext.SaveChanges();
				dbContext.Entry<T>(mainFile).State = EntityState.Detached;
				nextId = mainFile.NextId;
			}
		}

		private static FilesExtensionsResponse AddOrSaveFileHelper<T>(DbContext dbContext, Stream stream, string name, string mimeType = "application/octet-stream", Guid? fileId = null, int? chunkSize = null) where T : class, IFileEntity, new()
		{
			var bufferLen = (chunkSize.HasValue && chunkSize.Value > 0) ? chunkSize.Value : FileHelper.DEFAULT_MAX_CHUNK_SIZE;
			var nextId = Guid.NewGuid();
			if (!fileId.HasValue || fileId.Value.Equals(Guid.Empty))
			{
				fileId = Guid.NewGuid();
			}

			var filesExtensionsResponse = default(FilesExtensionsResponse);
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
					filesExtensionsResponse = new FilesExtensionsResponse
					{
						Id = fileId.Value,
						Name = tempT.Name,
						TotalBytesLength = tempT.TotalBytesLength,
						MimeType = tempT.MimeType,
						TimeStamp = tempT.TimeStamp
					};
				}
				if (stream.Length != stream.Position)
				{
					tempT.NextId = nextId;
				}
				dbContext.Add(tempT);
				dbContext.SaveChanges();
				dbContext.Entry<T>(tempT).State = EntityState.Detached;

			} while (stream.Length != stream.Position);

			return filesExtensionsResponse;
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
		[Obsolete("Use SaveFile(...) method")]
		public static FilesExtensionsResponse AddFile<T>(this DbContext dbContext, Stream stream, string name, string mimeType = "application/octet-stream", Guid? fileId = null, int? chunkSize = null) where T : class, IFileEntity, new()
		{
			return AddOrSaveFileHelper<T>(dbContext, stream, name, mimeType, fileId, chunkSize);
		}

		/// <summary>
		/// The DbContext method "SaveChanges()" will be called after every chunk, this is important
		/// if you want to avoid Entity Framework keeping so many large chunks in-memory.
		/// NOTE: Default chunk size = 64k
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dbContext"></param>
		/// <param name="filePath"></param>
		/// <param name="fileId"></param>
		/// <param name="chunkSize"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		[Obsolete("Use SaveFile(...) method")]
		public static FilesExtensionsResponse AddFileA<T>(this DbContext dbContext, string filePath, Guid? fileId = null, int? chunkSize = null, FileOptions options = FileOptions.None) where T : class, IFileEntity, new()
		{
			var bufferLen = (chunkSize.HasValue && chunkSize.Value > 0) ? chunkSize.Value : FileHelper.DEFAULT_MAX_CHUNK_SIZE;
			var fileInfo = new FileInfo(filePath);
			using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferLen, options);
			return AddOrSaveFileHelper<T>(dbContext, fileStream, fileInfo.Name, FileHelper.GetMimeType(fileInfo.Extension), fileId, chunkSize);
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
		public static FilesExtensionsResponse SaveFile<T>(this DbContext dbContext, Stream stream, string name, string mimeType = "application/octet-stream", Guid? fileId = null, int? chunkSize = null) where T : class, IFileEntity, new()
		{
			return AddOrSaveFileHelper<T>(dbContext, stream, name, mimeType, fileId, chunkSize);
		}

		/// <summary>
		/// The DbContext method "SaveChanges()" will be called after every chunk, this is important
		/// if you want to avoid Entity Framework keeping so many large chunks in-memory.
		/// NOTE: Default chunk size = 64k
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dbContext"></param>
		/// <param name="filePath"></param>
		/// <param name="fileId"></param>
		/// <param name="chunkSize"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static FilesExtensionsResponse SaveFile<T>(this DbContext dbContext, string filePath, Guid? fileId = null, int? chunkSize = null, FileOptions options = FileOptions.None) where T : class, IFileEntity, new()
		{
			var bufferLen = (chunkSize.HasValue && chunkSize.Value > 0) ? chunkSize.Value : FileHelper.DEFAULT_MAX_CHUNK_SIZE;
			var fileInfo = new FileInfo(filePath);
			using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferLen, options);
			return AddOrSaveFileHelper<T>(dbContext, fileStream, fileInfo.Name, FileHelper.GetMimeType(fileInfo.Extension), fileId, chunkSize);
		}
	}
}