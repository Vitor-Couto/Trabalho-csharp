using System.ComponentModel.DataAnnotations;

namespace MinhaApi.Models;

public class Categoria
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome da categoria é obrigatório.")]
    [MaxLength(50, ErrorMessage = "O nome da categoria deve ter no máximo 50 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    public List<Livro> Livros { get; set; } = new();
}
