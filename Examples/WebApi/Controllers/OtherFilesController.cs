using Files.EntityFrameworkCore.Extensions;
using Microsoft.AspNetCore.Mvc;
using Pagination.EntityFrameworkCore.Extensions;
using WebApi.Commands;
using WebApi.Data;
using WebApi.DTOs;
using WebApi.Entities;

namespace WebApi.Controllers
{
    [Route("api/other-files")]
    [ApiController]
    public class OtherFilesController : ControllerBase
    {
        private readonly WebApiContext _context;

        public OtherFilesController(WebApiContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<FilesExtensionsResponse>> UploadFile([FromBody] UploadFileIdCommand command)
        {
            var fileDetails = await _context.SaveFileAsync<OtherFile>(@"appsettings.json", command?.FileId);
            await _context.SaveChangesAsync();
            return Ok(fileDetails);
        }

        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownLoadFile(Guid id)
        {
            var fileDetails = await _context.GetFileInfoAsync<OtherFile>(id);
            var stream = new MemoryStream();
            await _context.DownloadFileToStreamAsync<OtherFile>(id, stream);
            return File(stream, fileDetails.MimeType, fileDetails.Name);
        }

        [HttpGet("{id}/view")]
        public async Task<FileStreamResult> DownloadView(Guid id)
        {
            var fileDetails = await _context.GetFileInfoAsync<OtherFile>(id);
            var stream = new MemoryStream();
            await _context.DownloadFileToStreamAsync<OtherFile>(id, stream);
            return File(stream, fileDetails.MimeType);
        }

        [HttpGet]
        public async Task<ActionResult<PaginationAuto<OtherFile, OtherFileDto>>> GetFiles(int page = 1, int limit = 20)
        {
            if (_context.UserImage == null)
            {
                return NotFound();
            }
            if (page <= 0)
            {
                page = 1;
            }
            var userImages = await _context.AsPaginationAsync<OtherFile, OtherFileDto>(page, limit, x => x.Id == x.FileId, ToDto);
            return Ok(userImages);
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

            await _context.DeleteFileAsync<OtherFile>(id);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private OtherFileDto ToDto(OtherFile userImage)
        {
            return new OtherFileDto
            {
                FileId = userImage.FileId,
                MimeType = userImage.MimeType,
                Name = userImage.Name,
                TimeStamp = userImage.TimeStamp,
                TotalBytesLength = userImage.TotalBytesLength,
            };
        }
    }
}