using Microsoft.AspNetCore.Mvc.Rendering;
using Turismo.Domain.Entities;

namespace Turismo.Models;

public class CostoFormViewModel
{
    public Costo Costo { get; set; } = new();
    public List<SelectListItem> Sitios { get; set; } = new();
    public Dictionary<int, string> SitioNombres { get; set; } = new();
}
