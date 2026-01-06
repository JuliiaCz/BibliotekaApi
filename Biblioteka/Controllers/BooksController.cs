using LibraryApi.Data;
using LibraryApi.Dtos;
using LibraryApi.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Controllers;

[ApiController]
[Route("books")]
public class BooksController : ControllerBase
{
    private readonly AppDbContext _db;

    public BooksController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<BookDto>>> GetAll([FromQuery] int? authorId)
    {
        var query = _db.Books
            .Include(b => b.Author)
            .AsQueryable();

        if (authorId.HasValue)
            query = query.Where(b => b.AuthorId == authorId.Value);

        var books = await query
            .OrderBy(b => b.Id)
            .Select(b => new BookDto
            {
                Id = b.Id,
                Title = b.Title,
                Year = b.Year,
                Author = new AuthorDto
                {
                    Id = b.Author!.Id,
                    FirstName = b.Author.FirstName,
                    LastName = b.Author.LastName
                }
            })
            .ToListAsync();

        return Ok(books);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookDto>> GetById(int id)
    {
        var b = await _db.Books
            .Include(x => x.Author)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (b == null || b.Author == null)
            return NotFound("Book not found");

        return Ok(new BookDto
        {
            Id = b.Id,
            Title = b.Title,
            Year = b.Year,
            Author = new AuthorDto
            {
                Id = b.Author.Id,
                FirstName = b.Author.FirstName,
                LastName = b.Author.LastName
            }
        });
    }

    [HttpPost]
    public async Task<ActionResult<BookDto>> Create([FromBody] BookCreateDto dto)
    {
        var title = (dto.Title ?? "").Trim();

        if (string.IsNullOrWhiteSpace(title))
            return BadRequest("title is required");

        if (dto.Year <= 0)
            return BadRequest("year must be > 0");

        var author = await _db.Authors.FirstOrDefaultAsync(a => a.Id == dto.AuthorId);
        if (author == null)
            return BadRequest("authorId must reference existing author");

        var entity = new BookEntity
        {
            Title = title,
            Year = dto.Year,
            AuthorId = dto.AuthorId
        };

        _db.Books.Add(entity);
        await _db.SaveChangesAsync();

        await _db.Entry(entity).Reference(e => e.Author).LoadAsync();

        var result = new BookDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Year = entity.Year,
            Author = new AuthorDto
            {
                Id = author.Id,
                FirstName = author.FirstName,
                LastName = author.LastName
            }
        };

        return Created($"/books/{entity.Id}", result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<BookDto>> Update(int id, [FromBody] BookUpdateDto dto)
    {
        if (dto.Id != id)
            return BadRequest("Body id must match route id");

        var title = (dto.Title ?? "").Trim();
        if (string.IsNullOrWhiteSpace(title))
            return BadRequest("title is required");

        if (dto.Year <= 0)
            return BadRequest("year must be > 0");

        var entity = await _db.Books.Include(b => b.Author).FirstOrDefaultAsync(b => b.Id == id);
        if (entity == null)
            return NotFound("Book not found");

        var author = await _db.Authors.FirstOrDefaultAsync(a => a.Id == dto.AuthorId);
        if (author == null)
            return BadRequest("authorId must reference existing author");

        entity.Title = title;
        entity.Year = dto.Year;
        entity.AuthorId = dto.AuthorId;

        await _db.SaveChangesAsync();
        return NoContent();


    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Books.FirstOrDefaultAsync(b => b.Id == id);
        if (entity == null)
            return NotFound("Book not found");

        _db.Books.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
