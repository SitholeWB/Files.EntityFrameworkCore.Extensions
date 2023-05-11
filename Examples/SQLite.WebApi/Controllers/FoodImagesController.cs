using Files.EntityFrameworkCore.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SQLite.WebApi.Commands;
using SQLite.WebApi.Data;
using SQLite.WebApi.DTOs;
using SQLite.WebApi.Entities;

namespace SQLite.WebApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class FoodImagesController : ControllerBase
	{
		private readonly SQLiteWebApiContext _context;

		public FoodImagesController(SQLiteWebApiContext context)
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
				var fileDetails = await _context.AddFileAsync<FoodImage>(file.OpenReadStream(), file.FileName, file.ContentType);
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
			var fileDetails = await _context.GetFileInfoAsync<FoodImage>(id);
			var stream = new MemoryStream();
			await _context.DownloadFileToStreamAsync<FoodImage>(id, stream);
			return File(stream, fileDetails.MimeType, fileDetails.Name);
		}

		[HttpGet("{id}/view")]
		public async Task<FileStreamResult> DownloadView(Guid id)
		{
			var fileDetails = await _context.GetFileInfoAsync<FoodImage>(id);
			var stream = new MemoryStream();
			await _context.DownloadFileToStreamAsync<FoodImage>(id, stream);
			return File(stream, fileDetails.MimeType);
		}

		[HttpGet]
		public async Task<ActionResult<FoodImageDto>> GetImages()
		{
			var foodImages = await _context.FoodImage.Where(x => x.Id == x.FileId).Select(x => new FoodImageDto
			{
				FileId = x.FileId,
				MimeType = x.MimeType,
				Name = x.Name,
				TimeStamp = x.TimeStamp,
				TotalBytesLength = x.TotalBytesLength,
			}).ToListAsync();
			return Ok(foodImages);
		}

		// DELETE: api/FoodImages/5
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteFoodImage(Guid id)
		{
			if (_context.FoodImage == null)
			{
				return NotFound();
			}
			var userImage = await _context.FoodImage.FindAsync(id);
			if (userImage == null)
			{
				return NotFound();
			}

			await _context.DeleteFileAsync<FoodImage>(id);
			await _context.SaveChangesAsync();

			return NoContent();
		}
	}
}