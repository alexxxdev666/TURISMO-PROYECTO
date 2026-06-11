namespace Turismo.Domain.Entities;

public class Multimedia
{
    public int Id { get; set; }
    public int SitioId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public int Orden { get; set; }
}
