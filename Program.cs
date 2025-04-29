using Microsoft.EntityFrameworkCore;
using MinhaApi.Data;
using MinhaApi.Models;
using System.ComponentModel.DataAnnotations; // Validações de dados

var builder = WebApplication.CreateBuilder(args);

// Banco de dados
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=banco.db"));

// Serviços básicos
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

// Swagger apenas no desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// LIVROS

app.MapGet("/livros", async (AppDbContext db) =>
    await db.Livros.Include(l => l.Categoria).ToListAsync());

app.MapPost("/livros", async (AppDbContext db, Livro livro) =>
{
    var validationResults = new List<ValidationResult>();
    var validationContext = new ValidationContext(livro);

    if (!Validator.TryValidateObject(livro, validationContext, validationResults, true))
    {
        var erros = validationResults.Select(v => v.ErrorMessage);
        return Results.BadRequest(erros);
    }

    var categoriaExiste = await db.Categorias.AnyAsync(c => c.Id == livro.CategoriaId);
    if (!categoriaExiste)
    {
        return Results.BadRequest($"Categoria com ID {livro.CategoriaId} não encontrada.");
    }

    db.Livros.Add(livro);
    await db.SaveChangesAsync();
    return Results.Created($"/livros/{livro.Id}", livro);
});

app.MapPut("/livros/{id}", async (int id, Livro input, AppDbContext db) =>
{
    var livro = await db.Livros.FindAsync(id);
    if (livro is null) return Results.NotFound();

    livro.Titulo = input.Titulo;
    livro.Autor = input.Autor;
    livro.Disponivel = input.Disponivel;
    livro.CategoriaId = input.CategoriaId;

    await db.SaveChangesAsync();
    return Results.Ok(livro);
});

app.MapDelete("/livros/{id}", async (int id, AppDbContext db) =>
{
    var livro = await db.Livros.FindAsync(id);
    if (livro is null) return Results.NotFound();

    db.Livros.Remove(livro);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapGet("/categorias/{id}/livros", async (int id, AppDbContext db) =>
{
    var categoria = await db.Categorias
        .Include(c => c.Livros)
        .FirstOrDefaultAsync(c => c.Id == id);

    return categoria is null
        ? Results.NotFound()
        : Results.Ok(categoria.Livros);
});

// CATEGORIAS

app.MapGet("/categorias", async (AppDbContext db) =>
    await db.Categorias.Include(c => c.Livros).ToListAsync());

app.MapPost("/categorias", async (AppDbContext db, Categoria categoria) =>
{
    var validationResults = new List<ValidationResult>();
    var validationContext = new ValidationContext(categoria);

    if (!Validator.TryValidateObject(categoria, validationContext, validationResults, true))
    {
        var erros = validationResults.Select(v => v.ErrorMessage);
        return Results.BadRequest(erros);
    }

    db.Categorias.Add(categoria);
    await db.SaveChangesAsync();
    return Results.Created($"/categorias/{categoria.Id}", categoria);
});

app.Run();
