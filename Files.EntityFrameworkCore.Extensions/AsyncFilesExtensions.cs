using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Files.EntityFrameworkCore.Extensions
{
    public static class AsyncFilesExtensions
    {
        /// <summary>
        /// Get the file info without bytes array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<FilesExtensionsResponse> GetFileInfoAsync<T>(this DbContext dbContext, Guid id, CancellationToken cancellationToken = default) where T : class, IFileEntity
        {
            return await dbContext.Set<T>().Select(x => new FilesExtensionsResponse
            {
                Name = x.Name,
                Id = x.Id,
                MimeType = x.MimeType,
                TimeStamp = x.TimeStamp,
                TotalBytesLength = x.TotalBytesLength,
            }).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
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

        /// <summary>
        /// Download the file to given stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="id">File Id</param>
        /// <param name="outputStream">Stream to receive file</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public static async Task DownloadFileToStreamAsync<T>(this DbContext dbContext, Guid id, Stream outputStream, CancellationToken cancellationToken = default) where T : class, IFileEntity
        {
            var mainFile = await dbContext.Set<T>().AsNoTracking().FirstOrDefaultAsync<T>(x => x.Id == id, cancellationToken);
            if (mainFile == null)
            {
                throw new FileNotFoundException();
            }
            await outputStream.WriteAsync(mainFile.Data, 0, mainFile.ChunkBytesLength, cancellationToken);
            var nextId = mainFile.NextId;
            while (mainFile.NextId.HasValue)
            {
                mainFile = await dbContext.Set<T>().AsNoTracking().FirstOrDefaultAsync<T>(x => x.Id == nextId, cancellationToken);
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

        /// <summary>
        /// Delete the file for given id, all chunks matching the id will be deleted.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public static async Task DeleteFileAsync<T>(this DbContext dbContext, Guid id, CancellationToken cancellationToken = default) where T : class, IFileEntity
        {
            var mainFile = await dbContext.Set<T>().FirstOrDefaultAsync<T>(x => x.Id == id, cancellationToken);
            if (mainFile == null)
            {
                throw new FileNotFoundException();
            }

            dbContext.Remove(mainFile);
            await dbContext.SaveChangesAsync();
            dbContext.Entry<T>(mainFile).State = EntityState.Detached;
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
                await dbContext.SaveChangesAsync();
                dbContext.Entry<T>(mainFile).State = EntityState.Detached;
                nextId = mainFile.NextId;
            }
        }

        private static async Task<FilesExtensionsResponse> AddOrSaveFileHelperAsync<T>(DbContext dbContext, Stream stream, string name, string mimeType = "application/octet-stream", Guid? fileId = null, int? chunkSize = null, bool eagerSave = false, CancellationToken cancellationToken = default) where T : class, IFileEntity, new()
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

                await stream.ReadAsync(buffer, 0, buffer.Length);
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
                if (eagerSave)
                {
                    await dbContext.SaveChangesAsync(cancellationToken);
                    dbContext.Entry<T>(tempT).State = EntityState.Detached;
                }
            } while (stream.Length != stream.Position);

            return filesExtensionsResponse;
        }

        /// <summary>
        /// The DbContext method "SaveChangesAsync()" will NOT be called after every chunk, this
        /// will cause Entity Framework to keep so many large chunks in-memory. You must explicitly
        /// call the SaveChangesAsync()
        /// NOTE: Default chunk size = 64k
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="stream"></param>
        /// <param name="name"></param>
        /// <param name="mimeType"></param>
        /// <param name="fileId"></param>
        /// <param name="chunkSize"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<FilesExtensionsResponse> AddFileAsync<T>(this DbContext dbContext, Stream stream, string name, string mimeType = "application/octet-stream", Guid? fileId = null, int? chunkSize = null, CancellationToken cancellationToken = default) where T : class, IFileEntity, new()
        {
            return await AddOrSaveFileHelperAsync<T>(dbContext, stream, name, mimeType, fileId, chunkSize, false, cancellationToken);
        }

        /// <summary>
        /// The DbContext method "SaveChangesAsync()" will NOT be called after every chunk, this
        /// will cause Entity Framework to keep so many large chunks in-memory. You must explicitly
        /// call the SaveChangesAsync()
        /// NOTE: Default chunk size = 64k
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="filePath"></param>
        /// <param name="fileId"></param>
        /// <param name="chunkSize"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<FilesExtensionsResponse> AddFileAsync<T>(this DbContext dbContext, string filePath, Guid? fileId = null, int? chunkSize = null, FileOptions options = FileOptions.None, CancellationToken cancellationToken = default) where T : class, IFileEntity, new()
        {
            var bufferLen = (chunkSize.HasValue && chunkSize.Value > 0) ? chunkSize.Value : FileHelper.DEFAULT_MAX_CHUNK_SIZE;
            var fileInfo = new FileInfo(filePath);
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferLen, options);
            return await AddOrSaveFileHelperAsync<T>(dbContext, fileStream, fileInfo.Name, FileHelper.GetMimeType(fileInfo.Extension), fileId, chunkSize, false, cancellationToken);
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
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<FilesExtensionsResponse> SaveFileAsync<T>(this DbContext dbContext, Stream stream, string name, string mimeType = "application/octet-stream", Guid? fileId = null, int? chunkSize = null, CancellationToken cancellationToken = default) where T : class, IFileEntity, new()
        {
            return await AddOrSaveFileHelperAsync<T>(dbContext, stream, name, mimeType, fileId, chunkSize, true, cancellationToken);
        }

        /// <summary>
        /// The DbContext method "SaveChangesAsync()" will be called after every chunk, this is
        /// important if you want to avoid Entity Framework keeping so many large chunks in-memory.
        /// NOTE: Default chunk size = 64k
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="filePath"></param>
        /// <param name="fileId"></param>
        /// <param name="chunkSize"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<FilesExtensionsResponse> SaveFileAsync<T>(this DbContext dbContext, string filePath, Guid? fileId = null, int? chunkSize = null, FileOptions options = FileOptions.None, CancellationToken cancellationToken = default) where T : class, IFileEntity, new()
        {
            var bufferLen = (chunkSize.HasValue && chunkSize.Value > 0) ? chunkSize.Value : FileHelper.DEFAULT_MAX_CHUNK_SIZE;
            var fileInfo = new FileInfo(filePath);
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferLen, options);
            return await AddOrSaveFileHelperAsync<T>(dbContext, fileStream, fileInfo.Name, FileHelper.GetMimeType(fileInfo.Extension), fileId, chunkSize, true, cancellationToken);
        }
    }
}