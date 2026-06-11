using Microsoft.AspNetCore.Mvc.Rendering;
using Turismo.Domain.Entities;

namespace Turismo.Models;

public class SitioFormViewModel
{
    public Sitio Sitio { get; set; } = new();
    public List<SelectListItem> Ubicaciones { get; set; } = new();
    public List<SitioComidaItemViewModel> Comidas { get; set; } = new();
}
