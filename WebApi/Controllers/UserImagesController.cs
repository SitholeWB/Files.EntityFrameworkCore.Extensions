using Files.EntityFrameworkCore.Extensions;
using Microsoft.AspNetCore.Mvc;
using WebApi.Commands;
using WebApi.Data;
using WebApi.Entities;

namespace WebApi.Controllers
{
	[Route("api/user-images")]
	[ApiController]
	public class UserImagesController : ControllerBase
	{
		private readonly WebApiContext _context;

		public UserImagesController(WebApiContext context)
		{
			_context = context;
		}

		//Example from https://dottutorials.net/dotnet-core-web-api-multipart-form-data-upload-file/
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

		// DELETE: api/UserImages/5
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
}