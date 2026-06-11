using Microsoft.AspNetCore.Mvc.Rendering;
using Turismo.Domain.Entities;

namespace Turismo.Models;

public class UbicacionFormViewModel
{
    public Ubicacion Ubicacion { get; set; } = new();
    public List<SelectListItem> Parents { get; set; } = new();
}
