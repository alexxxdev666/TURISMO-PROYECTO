namespace Turismo.Domain.Entities;

public class Auditoria
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Entidad { get; set; } = string.Empty;
    public int EntidadId { get; set; }
    public string Accion { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string Datos { get; set; } = string.Empty;
}
