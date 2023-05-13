# Files.EntityFrameworkCore.Extensions

This is a library for storing files in small chunks on sql using EntityFrameworkCore.
Works well with **Entity Framework** as an extension.

# Get Started

```nuget
Install-Package Files.EntityFrameworkCore.Extensions
```

# Example
### More examples found inside the repository 
```C#
  
  	public class UserImage : IFileEntity
	{
		public Guid Id { get; set; }
		public Guid FileId { get; set; }
		public string Name { get; set; }
		public string MimeType { get; set; }
		public DateTimeOffset TimeStamp { get; set; }
		public Guid? NextId { get; set; }
		public int ChunkBytesLength { get; set; }
		public long TotalBytesLength { get; set; }
		public byte[] Data { get; set; }
	}
	
	public class UploadCommand
	{
		public IFormFile File { get; set; }
	}
  
  	public class WebApiContext : DbContext
	{
		public WebApiContext(DbContextOptions<WebApiContext> options)
			: base(options)
		{
		}
		public DbSet<UserImage> UserImage { get; set; }
	}
  
        [Route("api/user-images")]
	[ApiController]
	public class UserImagesController : ControllerBase
	{
		private readonly WebApiContext _context;

		public UserImagesController(WebApiContext context)
		{
			_context = context;
		}

		[HttpPost]
		[DisableRequestSizeLimit]
		public async Task<ActionResult<FilesExtensionsResponse>> UploadFile([FromForm] UploadCommand uploadCommand)
		{
			var file = uploadCommand.File;
			if (file.Length > 0)
			{
				var fileDetails = await _context.AddFileAsync<UserImage>(file.OpenReadStream(), file.FileName, file.ContentType);
				await _context.SaveChangesAsync();
				return Ok(fileDetails);
			}
			else
			{
				return BadRequest("File is required.");
			}
		}

		[HttpGet("{id}/download")]
		public async Task<IActionResult> DownLoadFile(Guid id)
		{
			var fileDetails = await _context.GetFileInfoAsync<UserImage>(id);
			var stream = new MemoryStream();
			await _context.DownloadFileToStreamAsync<UserImage>(id, stream);
			return File(stream, fileDetails.MimeType, fileDetails.Name);
		}

		[HttpGet("{id}/view")]
		public async Task<FileStreamResult> DownloadView(Guid id)
		{
			var fileDetails = await _context.GetFileInfoAsync<UserImage>(id);
			var stream = new MemoryStream();
			await _context.DownloadFileToStreamAsync<UserImage>(id, stream);
			return new FileStreamResult(stream, fileDetails.MimeType);
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteUserImage(Guid id)
		{
			if (_context.UserImage == null)
			{
				return NotFound();
			}
			var userImage = await _context.UserImage.FindAsync(id);
			if (userImage == null)
			{
				return NotFound();
			}

			await _context.DeleteFileAsync<UserImage>(id);
			await _context.SaveChangesAsync();

			return NoContent();
		}
	}

```
