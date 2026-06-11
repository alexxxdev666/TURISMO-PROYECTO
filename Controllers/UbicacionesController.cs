using Microsoft.AspNetCore.Mvc;
using Turismo.Application.Services;
using Turismo.Domain.Entities;
using Turismo.Security;

namespace Turismo.Controllers;

[AdminOnly]
public class UbicacionesController : Controller
{
    private readonly UbicacionService _service;

    public UbicacionesController(UbicacionService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        var ubicaciones = await _service.GetAllAsync();
        ViewBag.ParentNombres = ubicaciones.ToDictionary(u => u.Id, u => $"{u.Tipo}: {u.Nombre}");
        return View(ubicaciones);
    }

    public async Task<IActionResult> Create(string? returnUrl = null)
    {
        var ubicaciones = await _service.GetAllAsync();
        ViewData["ReturnUrl"] = returnUrl;
        return View(new Models.UbicacionFormViewModel
        {
            Ubicacion = new Ubicacion(),
            Parents = ubicaciones.Select(u => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = u.Id.ToString(),
                Text = $"{u.Tipo}: {u.Nombre}"
            }).ToList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Models.UbicacionFormViewModel vm, string? returnUrl = null)
    {
        var ubicaciones = await _service.GetAllAsync();
        ValidateHierarchy(vm.Ubicacion, ubicaciones);

        if (!ModelState.IsValid)
        {
            vm.Parents = ubicaciones.Select(u => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = u.Id.ToString(),
                Text = $"{u.Tipo}: {u.Nombre}"
            }).ToList();
            ViewData["ReturnUrl"] = returnUrl;
            return View(vm);
        }

        await _service.CreateAsync(vm.Ubicacion);
        TempData["SuccessMessage"] = "Ubicación creada correctamente.";
        if (!string.IsNullOrWhiteSpace(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var ubicacion = await _service.GetByIdAsync(id);
        if (ubicacion == null)
        {
            return NotFound();
        }

        var allUbicaciones = await _service.GetAllAsync();
        ViewBag.UbicacionPadre = ubicacion.ParentId.HasValue
            ? allUbicaciones.FirstOrDefault(u => u.Id == ubicacion.ParentId.Value)
            : null;
        ViewBag.UbicacionCompleta = BuildUbicacionCompleta(ubicacion, allUbicaciones);
        ViewBag.EsParroquia = ubicacion.Tipo.Equals("Parroquia", StringComparison.OrdinalIgnoreCase);
        ViewBag.DestinoMapa = BuildDestinoMapa(ubicacion, allUbicaciones);
        return View(ubicacion);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var ubicacion = await _service.GetByIdAsync(id);
        if (ubicacion == null)
        {
            return NotFound();
        }

        var ubicaciones = await _service.GetAllAsync();
        ViewData["ReturnUrl"] = null;
        return View(new Models.UbicacionFormViewModel
        {
            Ubicacion = ubicacion,
            Parents = ubicaciones.Where(u => u.Id != id).Select(u => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = u.Id.ToString(),
                Text = $"{u.Tipo}: {u.Nombre}",
                Selected = u.Id == ubicacion.ParentId
            }).ToList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Models.UbicacionFormViewModel vm)
    {
        if (id != vm.Ubicacion.Id)
        {
            return BadRequest();
        }

        var ubicaciones = await _service.GetAllAsync();
        ValidateHierarchy(vm.Ubicacion, ubicaciones.Where(u => u.Id != id).ToList());

        if (!ModelState.IsValid)
        {
            vm.Parents = ubicaciones.Where(u => u.Id != id).Select(u => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = u.Id.ToString(),
                Text = $"{u.Tipo}: {u.Nombre}",
                Selected = u.Id == vm.Ubicacion.ParentId
            }).ToList();
            return View(vm);
        }

        await _service.UpdateAsync(vm.Ubicacion);
        TempData["SuccessMessage"] = "Ubicación actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var ubicacion = await _service.GetByIdAsync(id);
        return ubicacion == null ? NotFound() : View(ubicacion);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _service.DeleteAsync(id);
        TempData["SuccessMessage"] = "Ubicación eliminada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    private void ValidateHierarchy(Ubicacion ubicacion, IReadOnlyList<Ubicacion> ubicaciones)
    {
        var tipo = (ubicacion.Tipo ?? string.Empty).Trim();
        var parentId = ubicacion.ParentId;

        if (string.IsNullOrWhiteSpace(tipo))
        {
            ModelState.AddModelError("Ubicacion.Tipo", "El tipo es obligatorio.");
            return;
        }

        var isProvincia = tipo.Equals("Provincia", StringComparison.OrdinalIgnoreCase);
        var isCanton = tipo.Equals("Canton", StringComparison.OrdinalIgnoreCase) || tipo.Equals("Cantón", StringComparison.OrdinalIgnoreCase);
        var isParroquia = tipo.Equals("Parroquia", StringComparison.OrdinalIgnoreCase);

        if (!isProvincia && !isCanton && !isParroquia)
        {
            ModelState.AddModelError("Ubicacion.Tipo", "Tipo inválido. Use Provincia, Cantón o Parroquia.");
            return;
        }

        if (isProvincia)
        {
            if (parentId.HasValue)
            {
                ModelState.AddModelError("Ubicacion.ParentId", "Provincia no debe tener ubicación padre.");
            }
            return;
        }

        if (!parentId.HasValue)
        {
            ModelState.AddModelError("Ubicacion.ParentId", "Debe seleccionar una ubicación padre.");
            return;
        }

        var parent = ubicaciones.FirstOrDefault(u => u.Id == parentId.Value);
        if (parent == null)
        {
            ModelState.AddModelError("Ubicacion.ParentId", "La ubicación padre no existe.");
            return;
        }

        if (isCanton && !parent.Tipo.Equals("Provincia", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("Ubicacion.ParentId", "El cantón debe depender de una provincia.");
        }

        if (isParroquia && !(parent.Tipo.Equals("Canton", StringComparison.OrdinalIgnoreCase) || parent.Tipo.Equals("Cantón", StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError("Ubicacion.ParentId", "La parroquia debe depender de un cantón.");
        }
    }

    private static string BuildUbicacionCompleta(Ubicacion ubicacion, IReadOnlyList<Ubicacion> ubicaciones)
    {
        var nombres = new List<string> { ubicacion.Nombre };
        var actual = ubicacion;

        while (actual.ParentId.HasValue)
        {
            var padre = ubicaciones.FirstOrDefault(u => u.Id == actual.ParentId.Value);
            if (padre == null)
            {
                break;
            }

            nombres.Insert(0, padre.Nombre);
            actual = padre;
        }

        nombres.Add("Ecuador");
        return string.Join(", ", nombres);
    }

    private static string BuildDestinoMapa(Ubicacion ubicacion, IReadOnlyList<Ubicacion> ubicaciones)
    {
        var partes = new List<string>();

        if (!string.IsNullOrWhiteSpace(ubicacion.Nombre))
        {
            partes.Add(ubicacion.Nombre);
        }

        if (ubicacion.ParentId.HasValue)
        {
            var padre = ubicaciones.FirstOrDefault(u => u.Id == ubicacion.ParentId.Value);
            while (padre != null)
            {
                if (!string.IsNullOrWhiteSpace(padre.Nombre))
                {
                    partes.Add(padre.Nombre);
                }

                if (!padre.ParentId.HasValue)
                {
                    break;
                }

                padre = ubicaciones.FirstOrDefault(u => u.Id == padre.ParentId.Value);
            }
        }

        partes.Add("Ecuador");
        return string.Join(", ", partes);
    }
}
