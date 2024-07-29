using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.Models;

namespace Portfolio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorksController : ControllerBase
    {
        private readonly PortfolioDbContext _context;

        public WorksController(PortfolioDbContext context)
        {
            _context = context;
        }

        // GET: api/Works
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Work>>> GetWorks()
        {
            if (_context.Works == null)
            {
                return NotFound();
            }
            return await _context.Works.Include("Photos").ToListAsync();
        }

        // GET: api/Works/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Work>> GetWork(int id)
        {
            if (_context.Works == null)
            {
                return NotFound();
            }
            var work = await _context.Works.FindAsync(id);

            if (work == null)
            {
                return NotFound();
            }

            return work;
        }

        // PUT: api/Works/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutWork(int id, Work work)
        {
            if (id != work.Id)
            {
                return BadRequest();
            }

            _context.Entry(work).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WorkExists(id))
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

        // POST: api/Works
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPost]
        //public async Task<ActionResult<Work>> PostWork(Work work)
        //{
        //    if (_context.Works == null)
        //    {
        //        return Problem("Entity set 'PortfolioDbContext.Works'  is null.");
        //    }
        //    _context.Works.Add(work);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("GetWork", new { id = work.Id }, work);
        //}

        [HttpPost("workdata")]
        public async Task<ActionResult<Work>> PostWorkData([FromForm] Work work)
        {
            if (_context.Works == null)
            {
                return Problem("Entity set 'PortfolioDbContext.Works'  is null.");
            }

            //create a Photo list to store the upload files.  
            List<Photo> photolist = new List<Photo>();
            if (work.Files != null && work.Files.Count > 0)
            {
                foreach (var formFile in work.Files)
                {
                    if (formFile.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await formFile.CopyToAsync(memoryStream);
                            // Upload the file if less than 5 MB  
                            if (memoryStream.Length < 5242880)
                            {
                                //based on the upload file to create Photo instance.  
                                //You can also check the database, whether the image exists in the database.  
                                var newphoto = new Photo()
                                {
                                    Bytes = memoryStream.ToArray(),
                                    Description = formFile.FileName,
                                    FileExtension = Path.GetExtension(formFile.FileName),
                                    Size = formFile.Length,
                                };
                                //add the photo instance to the list.  
                                photolist.Add(newphoto);
                            }
                            else
                            {
                                ModelState.AddModelError("File", "The file is too large.");
                            }
                        }
                    }
                }
            }
            //assign the photos to the Product, using the navigation property.  
            work.Photos = photolist;

            _context.Works.Add(work);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetWork", new { id = work.Id }, work);
        }

        //[HttpPost]
        //public async Task<IActionResult> Post([FromForm(Name = "image")] IFormFile file)
        //{

        //    if (file == null || file.Length == 0)
        //    {
        //        return BadRequest("do not receive file");
        //    }

        //    var fileName = file.FileName;
        //    var extension = Path.GetExtension(fileName);
        //    var newFileName = $"{Guid.NewGuid()}{extension}";
        //    var filePath = Path.Combine(_env.ContentRootPath, "Images", newFileName);
        //    if (!Directory.Exists(Path.Combine(_env.ContentRootPath, "Images")))
        //    {
        //        Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Images"));
        //    }
        //    using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        //    {
        //        await file.CopyToAsync(stream);
        //    }

        //    return Ok(filePath);

        //}

        // DELETE: api/Works/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWork(int id)
        {
            if (_context.Works == null)
            {
                return NotFound();
            }
            var work = await _context.Works.FindAsync(id);
            if (work == null)
            {
                return NotFound();
            }

            _context.Works.Remove(work);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool WorkExists(int id)
        {
            return (_context.Works?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
