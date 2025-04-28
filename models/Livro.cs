using System.ComponentModel.DataAnnotations;

namespace MinhaApi.Models;

public class Livro
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O título é obrigatório.")]
    [MaxLength(100, ErrorMessage = "O título deve ter no máximo 100 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "O autor é obrigatório.")]
    [MaxLength(100, ErrorMessage = "O autor deve ter no máximo 100 caracteres.")]
    public string Autor { get; set; } = string.Empty;

    public bool Disponivel { get; set; }

    [Required(ErrorMessage = "CategoriaId é obrigatório.")]
    public int CategoriaId { get; set; }

    public Categoria? Categoria { get; set; }
}
