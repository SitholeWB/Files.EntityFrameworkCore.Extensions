using Files.EntityFrameworkCore.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySql.WebApi.Commands;
using MySql.WebApi.Data;
using MySql.WebApi.DTOs;
using MySql.WebApi.Entities;

namespace MySql.WebApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CarImagesController : ControllerBase
	{
		private readonly MySqlWebApiContext _context;

		public CarImagesController(MySqlWebApiContext context)
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
				var fileDetails = await _context.AddFileAsync<CarImage>(file.OpenReadStream(), file.FileName, file.ContentType);
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
			var fileDetails = await _context.GetFileInfoAsync<CarImage>(id);
			var stream = new MemoryStream();
			await _context.DownloadFileToStreamAsync<CarImage>(id, stream);
			return File(stream, fileDetails.MimeType, fileDetails.Name);
		}

		[HttpGet("{id}/view")]
		public async Task<FileStreamResult> DownloadView(Guid id)
		{
			var fileDetails = await _context.GetFileInfoAsync<CarImage>(id);
			var stream = new MemoryStream();
			await _context.DownloadFileToStreamAsync<CarImage>(id, stream);
			return File(stream, fileDetails.MimeType);
		}

		[HttpGet]
		public async Task<ActionResult<CarImageDto>> GetImages()
		{
			var carImages = await _context.CarImage.Where(x => x.Id == x.FileId).Select(x => new CarImageDto
			{
				FileId = x.FileId,
				MimeType = x.MimeType,
				Name = x.Name,
				TimeStamp = x.TimeStamp,
				TotalBytesLength = x.TotalBytesLength,
			}).ToListAsync();
			return Ok(carImages);
		}

		// DELETE: api/CarImages/5
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteCarImage(Guid id)
		{
			if (_context.CarImage == null)
			{
				return NotFound();
			}
			var userImage = await _context.CarImage.FindAsync(id);
			if (userImage == null)
			{
				return NotFound();
			}

			await _context.DeleteFileAsync<CarImage>(id);
			await _context.SaveChangesAsync();

			return NoContent();
		}
	}
}