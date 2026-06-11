namespace Turismo.Domain.Entities;

public class SitioComida
{
    public int SitioId { get; set; }
    public int ComidaId { get; set; }
    public decimal? ValorReferencial { get; set; }
    public string? Observacion { get; set; }
}
