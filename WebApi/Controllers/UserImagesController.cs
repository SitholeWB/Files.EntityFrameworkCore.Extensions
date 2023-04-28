using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        // GET: api/UserImages
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserImage>>> GetUserImage()
        {
          if (_context.UserImage == null)
          {
              return NotFound();
          }
            return await _context.UserImage.ToListAsync();
        }

        // GET: api/UserImages/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserImage>> GetUserImage(Guid id)
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

            return userImage;
        }

        // PUT: api/UserImages/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserImage(Guid id, UserImage userImage)
        {
            if (id != userImage.Id)
            {
                return BadRequest();
            }

            _context.Entry(userImage).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserImageExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/UserImages
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<UserImage>> PostUserImage(UserImage userImage)
        {
          if (_context.UserImage == null)
          {
              return Problem("Entity set 'WebApiContext.UserImage'  is null.");
          }
            _context.UserImage.Add(userImage);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUserImage", new { id = userImage.Id }, userImage);
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
