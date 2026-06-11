using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO;
using Turismo.Application.Services;
using Turismo.Domain.Entities;
using Turismo.Models;
using Turismo.Security;

namespace Turismo.Controllers;

[AdminOnly]
public class SitiosController : Controller
{
    private readonly SitioService _sitioService;
    private readonly UbicacionService _ubicacionService;
    private readonly CostoService _costoService;
    private readonly ComidaService _comidaService;
    private readonly MultimediaService _multimediaService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public SitiosController(SitioService sitioService, UbicacionService ubicacionService, CostoService costoService, ComidaService comidaService, MultimediaService multimediaService, IWebHostEnvironment webHostEnvironment)
    {
        _sitioService = sitioService;
        _ubicacionService = ubicacionService;
        _costoService = costoService;
        _comidaService = comidaService;
        _multimediaService = multimediaService;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IActionResult> Index()
    {
        var sitios = await _sitioService.GetAllAsync();
        var ubicaciones = await _ubicacionService.GetAllAsync();
        var ubicacionesPorId = ubicaciones.ToDictionary(ubicacion => ubicacion.Id);

        ViewBag.Ubicaciones = ubicacionesPorId;
        ViewBag.UbicacionesCompletas = sitios.ToDictionary(
            sitio => sitio.Id,
            sitio => ubicacionesPorId.TryGetValue(sitio.UbicacionId, out var ubicacion)
                ? BuildUbicacionCompleta(ubicacion, ubicaciones)
                : "Sin ubicacion asignada");
        ViewBag.TotalUbicaciones = ubicaciones.Count;

        return View(sitios);
    }

    public async Task<IActionResult> Details(int id)
    {
        var sitio = await _sitioService.GetByIdAsync(id);
        if (sitio == null)
        {
            return NotFound();
        }

        ViewBag.Imagenes = await _multimediaService.GetBySitioAsync(id);
        ViewBag.ComidaNombres = (await _comidaService.GetAllAsync()).ToDictionary(comida => comida.Id, comida => comida.Nombre);
        ViewBag.Comidas = await _comidaService.GetSitioComidasAsync(id);
        ViewBag.PromedioPersona = await CalcularPromedioPersonaAsync(id);
        return View(sitio);
    }

    public async Task<IActionResult> Create()
    {
        var vm = await BuildFormViewModelAsync(new Sitio());
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SitioFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm = await BuildFormViewModelAsync(vm.Sitio);
            return View(vm);
        }

        var sitioId = await _sitioService.CreateAsync(vm.Sitio);
        await _comidaService.ReplaceBySitioAsync(sitioId, BuildSitioComidas(sitioId, vm.Comidas));
        CrearCarpetaImagenesSitio(sitioId);
        TempData["SuccessMessage"] = "Sitio creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var sitio = await _sitioService.GetByIdAsync(id);
        if (sitio == null)
        {
            return NotFound();
        }

        var vm = await BuildFormViewModelAsync(sitio);
        return View(vm);
    }

    public async Task<IActionResult> AsociarComidas(int id)
    {
        var sitio = await _sitioService.GetByIdAsync(id);
        if (sitio == null)
        {
            return NotFound();
        }

        var vm = await BuildFormViewModelAsync(sitio);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AsociarComidas(int id, SitioFormViewModel vm)
    {
        if (id != vm.Sitio.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            vm = await BuildFormViewModelAsync(vm.Sitio);
            return View(vm);
        }

        await _comidaService.ReplaceBySitioAsync(vm.Sitio.Id, BuildSitioComidas(vm.Sitio.Id, vm.Comidas));
        TempData["SuccessMessage"] = "Las comidas del sitio se actualizaron correctamente.";
        return RedirectToAction(nameof(Details), new { id = vm.Sitio.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SitioFormViewModel vm)
    {
        if (id != vm.Sitio.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            vm = await BuildFormViewModelAsync(vm.Sitio);
            return View(vm);
        }

        await _sitioService.UpdateAsync(vm.Sitio);
        await _comidaService.ReplaceBySitioAsync(vm.Sitio.Id, BuildSitioComidas(vm.Sitio.Id, vm.Comidas));
        TempData["SuccessMessage"] = "Sitio actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var sitio = await _sitioService.GetByIdAsync(id);
        if (sitio == null)
        {
            return NotFound();
        }

        return View(sitio);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _sitioService.DeleteAsync(id);
        TempData["SuccessMessage"] = "Sitio eliminado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> UploadImages(int id)
    {
        var sitio = await _sitioService.GetByIdAsync(id);
        if (sitio == null)
        {
            return NotFound();
        }

        var imagenes = await _multimediaService.GetBySitioAsync(id);
        return View(new SitioMultimediaUploadViewModel
        {
            Sitio = sitio,
            Imagenes = imagenes.ToList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadImages(int id, SitioMultimediaUploadViewModel vm)
    {
        var sitio = await _sitioService.GetByIdAsync(id);
        if (sitio == null)
        {
            return NotFound();
        }

        if (vm.Archivos == null || vm.Archivos.Count == 0 || vm.Archivos.All(archivo => archivo == null || archivo.Length == 0))
        {
            ModelState.AddModelError(string.Empty, "Selecciona al menos una imagen para cargar.");
        }

        if (!ModelState.IsValid)
        {
            var imagenes = await _multimediaService.GetBySitioAsync(id);
            vm.Sitio = sitio;
            vm.Imagenes = imagenes.ToList();
            return View(vm);
        }

        var rutaSitio = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "sitios", id.ToString());
        Directory.CreateDirectory(rutaSitio);

        var imagenesExistentes = await _multimediaService.GetBySitioAsync(id);
        var siguienteOrden = imagenesExistentes.Count() == 0 ? 1 : imagenesExistentes.Max(imagen => imagen.Orden) + 1;

        foreach (var archivo in vm.Archivos!.Where(archivo => archivo != null && archivo.Length > 0))
        {
            var extension = Path.GetExtension(archivo.FileName);
            var nombreArchivo = $"{Guid.NewGuid():N}{extension}";
            var rutaCompleta = Path.Combine(rutaSitio, nombreArchivo);

            await using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            await _multimediaService.CreateAsync(new Multimedia
            {
                SitioId = id,
                Url = $"/uploads/sitios/{id}/{nombreArchivo}",
                Tipo = string.IsNullOrWhiteSpace(archivo.ContentType) ? "Imagen" : archivo.ContentType,
                Orden = siguienteOrden++
            });
        }

        TempData["SuccessMessage"] = "Las imágenes se cargaron correctamente.";
        return RedirectToAction(nameof(UploadImages), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteImage(int id, int imageId)
    {
        var sitio = await _sitioService.GetByIdAsync(id);
        if (sitio == null)
        {
            return NotFound();
        }

        var imagen = await _multimediaService.GetByIdAsync(imageId);
        if (imagen == null || imagen.SitioId != id)
        {
            TempData["ErrorMessage"] = "La imagen no existe o no pertenece al sitio.";
            return RedirectToAction(nameof(UploadImages), new { id });
        }

        try
        {
            EliminarArchivoImagen(imagen.Url);
            await _multimediaService.DeleteAsync(imageId);
            TempData["SuccessMessage"] = "La imagen se eliminó correctamente.";
        }
        catch
        {
            TempData["ErrorMessage"] = "No se pudo eliminar la imagen seleccionada.";
        }

        return RedirectToAction(nameof(UploadImages), new { id });
    }

    private async Task<SitioFormViewModel> BuildFormViewModelAsync(Sitio sitio)
    {
        var ubicaciones = await _ubicacionService.GetAllAsync();
        var comidas = await _comidaService.GetAllAsync();
        var comidasSitio = sitio.Id > 0
            ? (await _comidaService.GetSitioComidasAsync(sitio.Id)).ToDictionary(item => item.ComidaId)
            : new Dictionary<int, SitioComida>();

        return new SitioFormViewModel
        {
            Sitio = sitio,
            Ubicaciones = ubicaciones.Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = $"{u.Tipo}: {u.Nombre}",
                Selected = u.Id == sitio.UbicacionId
            }).ToList(),
            Comidas = comidas.Select(c => new Models.SitioComidaItemViewModel
            {
                ComidaId = c.Id,
                Nombre = c.Nombre,
                Seleccionada = comidasSitio.ContainsKey(c.Id),
                ValorReferencial = comidasSitio.TryGetValue(c.Id, out var relacion) ? relacion.ValorReferencial : null,
                Observacion = comidasSitio.TryGetValue(c.Id, out var relacionDetalle) ? relacionDetalle.Observacion : null
            }).ToList()
        };
    }

    private IReadOnlyList<SitioComida> BuildSitioComidas(int sitioId, IReadOnlyList<Models.SitioComidaItemViewModel> items)
    {
        var resultado = new List<SitioComida>();
        foreach (var item in items.Where(item => item.Seleccionada))
        {
            resultado.Add(new SitioComida
            {
                SitioId = sitioId,
                ComidaId = item.ComidaId,
                ValorReferencial = item.ValorReferencial,
                Observacion = item.Observacion
            });
        }

        return resultado;
    }

    private async Task<decimal> CalcularPromedioPersonaAsync(int sitioId)
    {
        var costos = await _costoService.GetBySitioAsync(sitioId);
        var comidas = await _comidaService.GetSitioComidasAsync(sitioId);

        var valores = costos.Select(costo => costo.Valor)
            .Concat(comidas.Where(comida => comida.ValorReferencial.HasValue).Select(comida => comida.ValorReferencial!.Value))
            .ToList();

        return valores.Count == 0 ? 0m : Math.Round(valores.Average(), 2);
    }

    private void CrearCarpetaImagenesSitio(int sitioId)
    {
        var rutaUploads = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "sitios", sitioId.ToString());
        Directory.CreateDirectory(rutaUploads);
    }

    private static string BuildUbicacionCompleta(Ubicacion ubicacion, IReadOnlyList<Ubicacion> ubicaciones)
    {
        var ubicacionesPorId = ubicaciones.ToDictionary(item => item.Id);
        var partes = new Stack<string>();
        var visitados = new HashSet<int>();
        Ubicacion? actual = ubicacion;

        while (actual != null && visitados.Add(actual.Id))
        {
            partes.Push($"{actual.Tipo}: {actual.Nombre}");

            if (!actual.ParentId.HasValue || !ubicacionesPorId.TryGetValue(actual.ParentId.Value, out var padre))
            {
                break;
            }

            actual = padre;
        }

        return string.Join(" > ", partes);
    }

    private void EliminarArchivoImagen(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        var rutaRelativa = url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var rutaCompleta = Path.Combine(_webHostEnvironment.WebRootPath, rutaRelativa);
        if (System.IO.File.Exists(rutaCompleta))
        {
            System.IO.File.Delete(rutaCompleta);
        }
    }
}
