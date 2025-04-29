using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinhaApi.Data;
using MinhaApi.Models;

namespace MinhaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LivrosController : ControllerBase
{
    private readonly AppDbContext _context;

    public LivrosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Livro>>> Get()
    {
        return await _context.Livros.Include(l => l.Categoria).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Livro>> Get(int id)
    {
        var livro = await _context.Livros.Include(l => l.Categoria).FirstOrDefaultAsync(l => l.Id == id);
        if (livro == null) return NotFound();
        return livro;
    }

    [HttpPost]
    public async Task<ActionResult<Livro>> Post(Livro livro)
    {
        _context.Livros.Add(livro);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = livro.Id }, livro);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, Livro livro)
    {
        if (id != livro.Id) return BadRequest();
        _context.Entry(livro).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var livro = await _context.Livros.FindAsync(id);
        if (livro == null) return NotFound();
        _context.Livros.Remove(livro);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
