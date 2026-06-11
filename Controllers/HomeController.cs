using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using turismo.Models;
using Turismo.Application.Services;
using System.IO;

namespace turismo.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly SitioService _sitioService;
    private readonly UbicacionService _ubicacionService;
    private readonly CostoService _costoService;
    private readonly ComidaService _comidaService;
    private readonly MultimediaService _multimediaService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public HomeController(
        ILogger<HomeController> logger,
        SitioService sitioService,
        UbicacionService ubicacionService,
        CostoService costoService,
        ComidaService comidaService,
        MultimediaService multimediaService,
        IWebHostEnvironment webHostEnvironment)
    {
        _logger = logger;
        _sitioService = sitioService;
        _ubicacionService = ubicacionService;
        _costoService = costoService;
        _comidaService = comidaService;
        _multimediaService = multimediaService;
        _webHostEnvironment = webHostEnvironment;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult AccessDenied(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // Página pública: Catálogo de sitios
    public async Task<IActionResult> Sitios()
    {
        var sitios = await _sitioService.GetAllAsync();
        var ubicacionesCatalogo = await _ubicacionService.GetAllAsync();
        var ubicacionesPorId = ubicacionesCatalogo.ToDictionary(ubicacion => ubicacion.Id);
        var portadas = new Dictionary<int, string>();
        var ubicaciones = new Dictionary<int, Turismo.Domain.Entities.Ubicacion?>();
        var ubicacionesCompletas = new Dictionary<int, string>();
        var rutas = new Dictionary<int, string>();

        foreach (var sitio in sitios)
        {
            var imagenes = await _multimediaService.GetBySitioAsync(sitio.Id);
            var portada = imagenes.OrderBy(imagen => imagen.Orden).FirstOrDefault();
            if (portada != null)
            {
                portadas[sitio.Id] = portada.Url;
            }

            ubicacionesPorId.TryGetValue(sitio.UbicacionId, out var ubicacion);
            ubicaciones[sitio.Id] = ubicacion;
            if (ubicacion != null)
            {
                ubicacionesCompletas[sitio.Id] = BuildUbicacionCompleta(ubicacion, ubicacionesCatalogo);
            }

            if (ubicacion?.Latitud.HasValue == true && ubicacion.Longitud.HasValue)
            {
                rutas[sitio.Id] = $"https://www.google.com/maps/dir/?api=1&destination={ubicacion.Latitud.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)},{ubicacion.Longitud.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
            }
        }

        ViewBag.Portadas = portadas;
        ViewBag.Ubicaciones = ubicaciones;
        ViewBag.UbicacionesCompletas = ubicacionesCompletas;
        ViewBag.Rutas = rutas;
        return View(sitios);
    }

    // Página pública: Detalle de sitio
    public async Task<IActionResult> SitioDetalle(int id)
    {
        var sitio = await _sitioService.GetByIdAsync(id);
        if (sitio == null)
        {
            return NotFound();
        }

        // Obtener ubicación
        var ubicacion = await _ubicacionService.GetByIdAsync(sitio.UbicacionId);
        var ubicacionesCatalogo = await _ubicacionService.GetAllAsync();
        
        // Obtener costos
        var costosSitio = (await _costoService.GetBySitioAsync(id)).ToList();

        // Obtener comidas asociadas al sitio con valor referencial
        var comidas = await _comidaService.GetSitioComidasAsync(id);
        var comidaCatalogo = (await _comidaService.GetAllAsync()).ToDictionary(comida => comida.Id, comida => comida.Nombre);
        var portadasComidas = comidaCatalogo.Keys.ToDictionary(
            comidaId => comidaId,
            comidaId => ObtenerPortadaComida(comidaId));

        var resumenPromedios = CalcularPromedios(costosSitio, comidas);

        var imagenes = await _multimediaService.GetBySitioAsync(id);

        ViewBag.Ubicacion = ubicacion;
        ViewBag.UbicacionCompleta = ubicacion == null ? null : BuildUbicacionCompleta(ubicacion, ubicacionesCatalogo);
        ViewBag.Costos = costosSitio;
        ViewBag.Comidas = comidas;
        ViewBag.ComidaNombres = comidaCatalogo;
        ViewBag.PortadasComidas = portadasComidas;
        ViewBag.Imagenes = imagenes;
        ViewBag.PromedioPersona = resumenPromedios.PromedioPersona;
        ViewBag.PromedioCostos = resumenPromedios.PromedioCostos;
        ViewBag.PromedioComidas = resumenPromedios.PromedioComidas;
        ViewBag.CantidadCostosPromediados = resumenPromedios.CantidadCostos;
        ViewBag.CantidadComidasPromediadas = resumenPromedios.CantidadComidas;

        return View(sitio);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private string? ObtenerPortadaComida(int comidaId)
    {
        var carpeta = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "comidas", comidaId.ToString());
        if (!Directory.Exists(carpeta))
        {
            return null;
        }

        var archivo = Directory.GetFiles(carpeta)
            .OrderBy(nombre => nombre)
            .Select(Path.GetFileName)
            .FirstOrDefault(nombre => !string.IsNullOrWhiteSpace(nombre));

        return string.IsNullOrWhiteSpace(archivo)
            ? null
            : $"/uploads/comidas/{comidaId}/{archivo}";
    }

    private static (decimal PromedioPersona, decimal PromedioCostos, decimal PromedioComidas, int CantidadCostos, int CantidadComidas)
        CalcularPromedios(IReadOnlyList<Turismo.Domain.Entities.Costo> costos, IReadOnlyList<Turismo.Domain.Entities.SitioComida> comidas)
    {
        var valoresCostos = costos.Select(costo => costo.Valor).ToList();
        var valoresComidas = comidas
            .Where(comida => comida.ValorReferencial.HasValue)
            .Select(comida => comida.ValorReferencial!.Value)
            .ToList();

        var promedioCostos = valoresCostos.Count == 0 ? 0m : Math.Round(valoresCostos.Average(), 2);
        var promedioComidas = valoresComidas.Count == 0 ? 0m : Math.Round(valoresComidas.Average(), 2);

        decimal promedioPersona;

        if (valoresCostos.Count > 0 && valoresComidas.Count > 0)
        {
            promedioPersona = Math.Round((promedioCostos + promedioComidas) / 2m, 2);
        }
        else if (valoresCostos.Count > 0)
        {
            promedioPersona = promedioCostos;
        }
        else if (valoresComidas.Count > 0)
        {
            promedioPersona = promedioComidas;
        }
        else
        {
            promedioPersona = 0m;
        }

        return (promedioPersona, promedioCostos, promedioComidas, valoresCostos.Count, valoresComidas.Count);
    }

    private static string BuildUbicacionCompleta(
        Turismo.Domain.Entities.Ubicacion ubicacion,
        IReadOnlyList<Turismo.Domain.Entities.Ubicacion> ubicaciones)
    {
        var nombres = new List<string>();
        var actual = ubicacion;

        while (actual != null)
        {
            if (!string.IsNullOrWhiteSpace(actual.Nombre))
            {
                nombres.Add(actual.Nombre);
            }

            if (!actual.ParentId.HasValue)
            {
                break;
            }

            actual = ubicaciones.FirstOrDefault(item => item.Id == actual.ParentId.Value);
        }

        nombres.Reverse();
        nombres.Insert(0, "Ecuador");
        return string.Join(" > ", nombres);
    }
}
