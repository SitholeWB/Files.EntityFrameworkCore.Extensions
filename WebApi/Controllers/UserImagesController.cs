using Files.EntityFrameworkCore.Extensions;
using Microsoft.AspNetCore.Mvc;
using WebApi.Commands;
using WebApi.Data;
using WebApi.Entities;

namespace WebApi.Controllers
{
	[Route("api/[controller]")]
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
				var fileDetails = await _context.SaveFileAsync<UserImage>(file.OpenReadStream(), file.FileName, file.ContentType);
				await _context.SaveChangesAsync();
				return Ok(fileDetails);
			}
			else
			{
				return BadRequest("File is required.");
			}
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> DownLoadFile(Guid id)
		{
			var response = await _context.GetFileStreamAsync<UserImage>(id);
			return File(response.Stream, response.MimeType, response.Name);
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

			_context.UserImage.Remove(userImage);
			await _context.SaveChangesAsync();

			return NoContent();
		}

		private bool UserImageExists(Guid id)
		{
			return (_context.UserImage?.Any(e => e.Id == id)).GetValueOrDefault();
		}
	}
}