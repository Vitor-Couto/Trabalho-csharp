using Microsoft.EntityFrameworkCore;
using MinhaApi.Data;
using MinhaApi.Models;
using System.ComponentModel.DataAnnotations; // <--- Adicionado

var builder = WebApplication.CreateBuilder(args);

// Configura o banco de dados SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=banco.db"));

// Configurações básicas
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configura Swagger para ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Rotas para LIVROS

// Buscar todos os livros
app.MapGet("/livros", async (AppDbContext db) =>
    await db.Livros.Include(l => l.Categoria).ToListAsync());

// Criar um novo livro
app.MapPost("/livros", async (AppDbContext db, Livro livro) =>
{
    // Validação de DataAnnotations
    var validationResults = new List<ValidationResult>();
    var validationContext = new ValidationContext(livro);

    if (!Validator.TryValidateObject(livro, validationContext, validationResults, true))
    {
        var erros = validationResults.Select(v => v.ErrorMessage);
        return Results.BadRequest(erros);
    }

    // Validação da existência da Categoria
    var categoriaExiste = await db.GetCategorias().AnyAsync(c => c.Id == livro.CategoriaId);
    if (!categoriaExiste)
    {
        return Results.BadRequest($"Categoria com ID {livro.CategoriaId} não existe.");
    }

    db.Livros.Add(livro);
    await db.SaveChangesAsync();
    return Results.Created($"/livros/{livro.Id}", livro);
});

// Atualizar um livro existente
app.MapPut("/livros/{id}", async (int id, Livro input, AppDbContext db) =>
{
    var livro = await db.Livros.FindAsync(id);
    if (livro is null) return Results.NotFound();

    // Atualizar dados
    livro.Titulo = input.Titulo;
    livro.Autor = input.Autor;
    livro.Disponivel = input.Disponivel;
    livro.CategoriaId = input.CategoriaId;

    await db.SaveChangesAsync();
    return Results.Ok(livro);
});

// Deletar um livro
app.MapDelete("/livros/{id}", async (int id, AppDbContext db) =>
{
    var livro = await db.Livros.FindAsync(id);
    if (livro is null) return Results.NotFound();

    db.Livros.Remove(livro);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Buscar todos os livros de uma categoria específica
app.MapGet("/categorias/{id}/livros", async (int id, AppDbContext db) =>
{
    var categoria = await db.GetCategorias()
        .Include(c => c.Livros)
        .FirstOrDefaultAsync(c => c.Id == id);

    return categoria is null
        ? Results.NotFound()
        : Results.Ok(categoria.Livros);
});

// Rotas para CATEGORIAS

// Buscar todas as categorias
app.MapGet("/categorias", async (AppDbContext db) =>
    await db.GetCategorias().Include(c => c.Livros).ToListAsync());

// Criar uma nova categoria
app.MapPost("/categorias", async (AppDbContext db, Categoria categoria) =>
{
    // Validação de DataAnnotations
    var validationResults = new List<ValidationResult>();
    var validationContext = new ValidationContext(categoria);

    if (!Validator.TryValidateObject(categoria, validationContext, validationResults, true))
    {
        var erros = validationResults.Select(v => v.ErrorMessage);
        return Results.BadRequest(erros);
    }

    db.GetCategorias().Add(categoria);
    await db.SaveChangesAsync();
    return Results.Created($"/categorias/{categoria.Id}", categoria);
});

app.Run();
