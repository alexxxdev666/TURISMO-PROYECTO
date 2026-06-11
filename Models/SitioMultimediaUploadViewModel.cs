using Microsoft.AspNetCore.Http;
using Turismo.Domain.Entities;

namespace Turismo.Models;

public class SitioMultimediaUploadViewModel
{
    public Sitio Sitio { get; set; } = new();
    public List<Multimedia> Imagenes { get; set; } = [];
    public List<IFormFile> Archivos { get; set; } = [];
}