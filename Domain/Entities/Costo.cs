namespace Turismo.Domain.Entities;

public class Costo
{
    public int Id { get; set; }
    public int SitioId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string Moneda { get; set; } = "USD";
    public string? Observacion { get; set; }
}
