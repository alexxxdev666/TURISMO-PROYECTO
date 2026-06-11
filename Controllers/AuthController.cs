using Microsoft.AspNetCore.Mvc;
using Turismo.Application.Services;
using Turismo.Models;
using Turismo.Security;

namespace Turismo.Controllers;

public class AuthController : Controller
{
    private readonly UsuarioService _usuarioService;
    private readonly PasswordHasher _passwordHasher;

    public AuthController(UsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
        _passwordHasher = new PasswordHasher();
    }

    [HttpGet]
    public IActionResult Login(bool accessDenied = false, string? returnUrl = null)
    {
        if (HttpContext.Session.GetInt32("UserId").HasValue &&
            string.Equals(HttpContext.Session.GetString("UserEmail"), AdminOnlyAttribute.AdminEmail, StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction("Index", "Home");
        }

        if (accessDenied)
        {
            ViewData["AccessMessage"] = "Debes iniciar sesión como Administrador para acceder a esta sección.";
        }

        if (TempData.TryGetValue("AuthMessage", out var authMessage))
        {
            ViewData["AuthMessage"] = authMessage?.ToString();
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        var returnUrl = Request.Form["returnUrl"].ToString();

        if (!ModelState.IsValid)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(vm);
        }

        var usuario = await _usuarioService.GetByEmailAsync(vm.Email);
        if (usuario == null ||
            !usuario.Activo ||
            !string.Equals(usuario.Email, AdminOnlyAttribute.AdminEmail, StringComparison.OrdinalIgnoreCase) ||
            !_passwordHasher.Verify(vm.Password, usuario.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "Solo el administrador puede ingresar al panel.");
            ViewData["ReturnUrl"] = returnUrl;
            return View(vm);
        }

        HttpContext.Session.SetInt32("UserId", usuario.Id);
        HttpContext.Session.SetString("UserName", usuario.Nombre);
        HttpContext.Session.SetString("UserEmail", usuario.Email);
        TempData["StatusMessage"] = "Sesión de administrador iniciada correctamente.";

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        TempData["AuthMessage"] = "La sesión de administrador se cerró correctamente.";
        return RedirectToAction(nameof(Login));
    }
}
