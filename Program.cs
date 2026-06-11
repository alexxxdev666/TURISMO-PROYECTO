using Turismo.Application.Interfaces;
using Turismo.Application.Services;
using Turismo.Infrastructure.Data;
using Turismo.Infrastructure.Repositories;
using Turismo.Domain.Entities;
using Turismo.Security;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<IDbConnectionFactory, OleDbConnectionFactory>();
builder.Services.AddScoped<IUbicacionRepository, UbicacionRepository>();
builder.Services.AddScoped<ISitioRepository, SitioRepository>();
builder.Services.AddScoped<IMultimediaRepository, MultimediaRepository>();
builder.Services.AddScoped<ICostoRepository, CostoRepository>();
builder.Services.AddScoped<IComidaRepository, ComidaRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<UbicacionService>();
builder.Services.AddScoped<SitioService>();
builder.Services.AddScoped<MultimediaService>();
builder.Services.AddScoped<CostoService>();
builder.Services.AddScoped<ComidaService>();
builder.Services.AddScoped<UsuarioService>();

builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromHours(8);
});

var app = builder.Build();

AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(builder.Environment.ContentRootPath, "data"));

await SeedAdminUserAsync(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

static async Task SeedAdminUserAsync(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var usuarioService = scope.ServiceProvider.GetRequiredService<UsuarioService>();

    var adminEmail = "admin@turismo.local";
    var existingAdmin = await usuarioService.GetByEmailAsync(adminEmail);
    if (existingAdmin != null)
    {
        return;
    }

    var passwordHasher = new PasswordHasher();
    await usuarioService.CreateAsync(new Usuario
    {
        Nombre = "Administrador",
        Email = adminEmail,
        PasswordHash = passwordHasher.Hash("Admin2628"),
        Activo = true
    });
}
