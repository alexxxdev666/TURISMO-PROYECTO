using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Turismo.Application.Services;
using Turismo.Domain.Entities;
using Turismo.Models;
using Turismo.Security;

namespace Turismo.Controllers;

[AdminOnly]
public class CostosController : Controller
{
    private readonly CostoService _costoService;
    private readonly SitioService _sitioService;

    public CostosController(CostoService costoService, SitioService sitioService)
    {
        _costoService = costoService;
        _sitioService = sitioService;
    }

    public async Task<IActionResult> Index()
    {
        var costos = await _costoService.GetAllAsync();
        var sitios = await _sitioService.GetAllAsync();
        ViewBag.SitioNombres = sitios.ToDictionary(s => s.Id, s => s.Nombre);
        return View(costos);
    }

    public async Task<IActionResult> Create()
    {
        return View(await BuildViewModelAsync(new Costo()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CostoFormViewModel vm)
    {
        await ValidarRelacionSitioCostoAsync(vm.Costo);

        if (!ModelState.IsValid)
        {
            vm = await BuildViewModelAsync(vm.Costo);
            return View(vm);
        }

        await _costoService.CreateAsync(vm.Costo);
        TempData["SuccessMessage"] = "Costo registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var costo = await _costoService.GetByIdAsync(id);
        if (costo == null)
        {
            return NotFound();
        }

        return View(await BuildViewModelAsync(costo));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CostoFormViewModel vm)
    {
        if (id != vm.Costo.Id)
        {
            return BadRequest();
        }

        await ValidarRelacionSitioCostoAsync(vm.Costo);

        if (!ModelState.IsValid)
        {
            vm = await BuildViewModelAsync(vm.Costo);
            return View(vm);
        }

        await _costoService.UpdateAsync(vm.Costo);
        TempData["SuccessMessage"] = "Costo actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var costo = await _costoService.GetByIdAsync(id);
        return costo == null ? NotFound() : View(costo);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _costoService.DeleteAsync(id);
        TempData["SuccessMessage"] = "Costo eliminado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<CostoFormViewModel> BuildViewModelAsync(Costo costo)
    {
        var sitios = await _sitioService.GetAllAsync();
        return new CostoFormViewModel
        {
            Costo = costo,
            Sitios = sitios.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.Nombre,
                Selected = s.Id == costo.SitioId
            }).ToList(),
            SitioNombres = sitios.ToDictionary(s => s.Id, s => s.Nombre)
        };
    }

    private async Task ValidarRelacionSitioCostoAsync(Costo costo)
    {
        if (costo.SitioId <= 0)
        {
            ModelState.AddModelError("Costo.SitioId", "Debe seleccionar un sitio.");
            return;
        }

        var sitio = await _sitioService.GetByIdAsync(costo.SitioId);
        if (sitio == null)
        {
            ModelState.AddModelError("Costo.SitioId", "El sitio seleccionado no existe o fue eliminado.");
        }
    }
}
