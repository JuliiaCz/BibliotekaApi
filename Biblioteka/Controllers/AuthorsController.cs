using LibraryApi.Data;
using LibraryApi.Dtos;
using LibraryApi.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Controllers;

[ApiController]
[Route("authors")]
public class AuthorsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AuthorsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuthorDto>>> GetAll()
    {
        var authors = await _db.Authors
            .OrderBy(a => a.Id)
            .Select(a => new AuthorDto
            {
                Id = a.Id,
                FirstName = a.FirstName,
                LastName = a.LastName
            })
            .ToListAsync();

        return Ok(authors);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AuthorDto>> GetById(int id)
    {
        var a = await _db.Authors.FirstOrDefaultAsync(x => x.Id == id);
        if (a == null)
            return NotFound("Author not found");

        return Ok(new AuthorDto
        {
            Id = a.Id,
            FirstName = a.FirstName,
            LastName = a.LastName
        });
    }

    [HttpPost]
    public async Task<ActionResult<AuthorDto>> Create([FromBody] AuthorCreateDto dto)
    {
        var first = (dto.FirstName ?? "").Trim();
        var last = (dto.LastName ?? "").Trim();

        if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(last))
            return BadRequest("first_name and last_name are required");

        var entity = new AuthorEntity
        {
            FirstName = first,
            LastName = last
        };

        _db.Authors.Add(entity);
        await _db.SaveChangesAsync();

        var result = new AuthorDto
        {
            Id = entity.Id,
            FirstName = entity.FirstName,
            LastName = entity.LastName
        };

        return Created($"/authors/{entity.Id}", result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AuthorDto>> Update(int id, [FromBody] AuthorUpdateDto dto)
    {
        if (dto.Id != id)
            return BadRequest("Body id must match route id");

        var first = (dto.FirstName ?? "").Trim();
        var last = (dto.LastName ?? "").Trim();

        if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(last))
            return BadRequest("first_name and last_name are required");

        var entity = await _db.Authors.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null)
            return NotFound("Author not found");

        entity.FirstName = first;
        entity.LastName = last;

        await _db.SaveChangesAsync();

        return Ok(new AuthorDto
        {
            Id = entity.Id,
            FirstName = entity.FirstName,
            LastName = entity.LastName
        });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Authors
            .Include(a => a.Books)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return NotFound("Author not found");

        _db.Books.RemoveRange(entity.Books);
        _db.Authors.Remove(entity);

        await _db.SaveChangesAsync();
        return NoContent();
    }
}
