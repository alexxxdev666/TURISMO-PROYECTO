using Microsoft.AspNetCore.Mvc;
using Turismo.Application.Services;
using Turismo.Models;
using Turismo.Security;

namespace Turismo.Controllers;

[AdminOnly]
public class UsuariosController : Controller
{
    private readonly UsuarioService _service;
    private readonly PasswordHasher _passwordHasher;

    public UsuariosController(UsuarioService service)
    {
        _service = service;
        _passwordHasher = new PasswordHasher();
    }

    public async Task<IActionResult> Index()
    {
        return View(await _service.GetAllAsync());
    }

    public IActionResult Create() => View(new UsuarioFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UsuarioFormViewModel vm)
    {
        if (string.IsNullOrWhiteSpace(vm.Password) || vm.Password != vm.ConfirmPassword)
        {
            ModelState.AddModelError(nameof(vm.Password), "Las contraseñas no coinciden.");
        }

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        vm.Usuario.PasswordHash = _passwordHasher.Hash(vm.Password!);
        await _service.CreateAsync(vm.Usuario);
        TempData["SuccessMessage"] = "Usuario creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var usuario = await _service.GetByIdAsync(id);
        return usuario == null ? NotFound() : View(new UsuarioFormViewModel { Usuario = usuario });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UsuarioFormViewModel vm)
    {
        if (id != vm.Usuario.Id)
        {
            return BadRequest();
        }

        if (!string.IsNullOrWhiteSpace(vm.Password) || !string.IsNullOrWhiteSpace(vm.ConfirmPassword))
        {
            if (vm.Password != vm.ConfirmPassword)
            {
                ModelState.AddModelError(nameof(vm.Password), "Las contraseñas no coinciden.");
            }
            else if (!string.IsNullOrWhiteSpace(vm.Password))
            {
                vm.Usuario.PasswordHash = _passwordHasher.Hash(vm.Password);
            }
        }

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        if (string.IsNullOrWhiteSpace(vm.Usuario.PasswordHash))
        {
            var current = await _service.GetByIdAsync(id);
            if (current == null)
            {
                return NotFound();
            }
            vm.Usuario.PasswordHash = current.PasswordHash;
        }

        await _service.UpdateAsync(vm.Usuario);
        TempData["SuccessMessage"] = "Usuario actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var usuario = await _service.GetByIdAsync(id);
        return usuario == null ? NotFound() : View(usuario);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _service.DeleteAsync(id);
        TempData["SuccessMessage"] = "Usuario eliminado correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
