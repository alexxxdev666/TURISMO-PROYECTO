using Microsoft.AspNetCore.Mvc;
using System.IO;
using Turismo.Application.Services;
using Turismo.Models;
using Turismo.Security;

namespace Turismo.Controllers;

[AdminOnly]
public class ComidasController : Controller
{
    private readonly ComidaService _service;
    private readonly SitioService _sitioService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ComidasController(ComidaService service, SitioService sitioService, IWebHostEnvironment webHostEnvironment)
    {
        _service = service;
        _sitioService = sitioService;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IActionResult> Index()
    {
        var comidas = await _service.GetAllAsync();
        var sitios = await _sitioService.GetAllAsync();
        var sitiosPorComida = comidas.ToDictionary(comida => comida.Id, _ => new List<string>());

        foreach (var sitio in sitios)
        {
            var relaciones = await _service.GetSitioComidasAsync(sitio.Id);
            foreach (var relacion in relaciones)
            {
                if (sitiosPorComida.TryGetValue(relacion.ComidaId, out var nombresSitios))
                {
                    nombresSitios.Add(sitio.Nombre);
                }
            }
        }

        ViewBag.SitiosPorComida = sitiosPorComida.ToDictionary(
            item => item.Key,
            item => item.Value.Distinct().OrderBy(nombre => nombre).ToList());
        ViewBag.PortadasComidas = comidas.ToDictionary(comida => comida.Id, comida => ObtenerPortadaComida(comida.Id));

        return View(comidas);
    }

    public IActionResult Create() => View(new ComidaFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ComidaFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var comidaId = await _service.CreateAsync(vm.Comida);
        CrearCarpetaImagenesComida(comidaId);
        TempData["SuccessMessage"] = "Comida creada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var comida = await _service.GetByIdAsync(id);
        return comida == null ? NotFound() : View(new ComidaFormViewModel { Comida = comida });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ComidaFormViewModel vm)
    {
        if (id != vm.Comida.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        await _service.UpdateAsync(vm.Comida);
        TempData["SuccessMessage"] = "Comida actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var comida = await _service.GetByIdAsync(id);
        return comida == null ? NotFound() : View(comida);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _service.DeleteAsync(id);
        EliminarCarpetaImagenesComida(id);
        TempData["SuccessMessage"] = "Comida eliminada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> UploadImages(int id)
    {
        var comida = await _service.GetByIdAsync(id);
        if (comida == null)
        {
            return NotFound();
        }

        return View(new ComidaMultimediaUploadViewModel
        {
            Comida = comida,
            Imagenes = ObtenerImagenesComida(id)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadImages(int id, ComidaMultimediaUploadViewModel vm)
    {
        var comida = await _service.GetByIdAsync(id);
        if (comida == null)
        {
            return NotFound();
        }

        if (vm.Archivos == null || vm.Archivos.Count == 0 || vm.Archivos.All(archivo => archivo == null || archivo.Length == 0))
        {
            ModelState.AddModelError(string.Empty, "Selecciona al menos una imagen para cargar.");
        }

        if (!ModelState.IsValid)
        {
            vm.Comida = comida;
            vm.Imagenes = ObtenerImagenesComida(id);
            return View(vm);
        }

        var rutaComida = ObtenerRutaCarpetaComida(id);
        Directory.CreateDirectory(rutaComida);

        foreach (var archivo in vm.Archivos!.Where(archivo => archivo != null && archivo.Length > 0))
        {
            var extension = Path.GetExtension(archivo.FileName);
            var nombreArchivo = $"{Guid.NewGuid():N}{extension}";
            var rutaCompleta = Path.Combine(rutaComida, nombreArchivo);

            await using var stream = new FileStream(rutaCompleta, FileMode.Create);
            await archivo.CopyToAsync(stream);
        }

        TempData["SuccessMessage"] = "Las imágenes de la comida se cargaron correctamente.";
        return RedirectToAction(nameof(UploadImages), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteImage(int id, string fileName)
    {
        var comida = await _service.GetByIdAsync(id);
        if (comida == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            TempData["ErrorMessage"] = "Nombre de archivo inválido.";
            return RedirectToAction(nameof(UploadImages), new { id });
        }

        var nombreSeguro = Path.GetFileName(fileName);
        var rutaArchivo = Path.Combine(ObtenerRutaCarpetaComida(id), nombreSeguro);
        if (System.IO.File.Exists(rutaArchivo))
        {
            System.IO.File.Delete(rutaArchivo);
            TempData["SuccessMessage"] = "La imagen se eliminó correctamente.";
        }
        else
        {
            TempData["ErrorMessage"] = "No se encontró la imagen seleccionada.";
        }

        return RedirectToAction(nameof(UploadImages), new { id });
    }

    private void CrearCarpetaImagenesComida(int comidaId)
    {
        Directory.CreateDirectory(ObtenerRutaCarpetaComida(comidaId));
    }

    private void EliminarCarpetaImagenesComida(int comidaId)
    {
        var ruta = ObtenerRutaCarpetaComida(comidaId);
        if (Directory.Exists(ruta))
        {
            Directory.Delete(ruta, true);
        }
    }

    private List<string> ObtenerImagenesComida(int comidaId)
    {
        var ruta = ObtenerRutaCarpetaComida(comidaId);
        if (!Directory.Exists(ruta))
        {
            return [];
        }

        return Directory
            .GetFiles(ruta)
            .Select(Path.GetFileName)
            .Where(nombre => !string.IsNullOrWhiteSpace(nombre))
            .Select(nombre => $"/uploads/comidas/{comidaId}/{nombre}")
            .OrderBy(url => url)
            .ToList();
    }

    private string? ObtenerPortadaComida(int comidaId)
    {
        return ObtenerImagenesComida(comidaId).FirstOrDefault();
    }

    private string ObtenerRutaCarpetaComida(int comidaId)
    {
        return Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "comidas", comidaId.ToString());
    }
}
