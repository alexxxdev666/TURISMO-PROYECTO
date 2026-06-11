using Turismo.Domain.Entities;

namespace Turismo.Models;

public class UsuarioFormViewModel
{
    public Usuario Usuario { get; set; } = new();
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }
}
