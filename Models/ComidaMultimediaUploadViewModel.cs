using Microsoft.AspNetCore.Http;
using Turismo.Domain.Entities;

namespace Turismo.Models;

public class ComidaMultimediaUploadViewModel
{
    public Comida Comida { get; set; } = new();
    public List<string> Imagenes { get; set; } = [];
    public List<IFormFile> Archivos { get; set; } = [];
}