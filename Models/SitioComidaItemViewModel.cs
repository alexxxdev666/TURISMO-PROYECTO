namespace Turismo.Models;

public class SitioComidaItemViewModel
{
    public int ComidaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Seleccionada { get; set; }
    public decimal? ValorReferencial { get; set; }
    public string? Observacion { get; set; }
}