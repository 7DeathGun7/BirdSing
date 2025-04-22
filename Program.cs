using BirdSing.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configurar la conexión a la base de datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar autenticación con cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Redirigir a Login si no está autenticado
        options.AccessDeniedPath = "/Account/AccessDenied"; // Redirigir a error de acceso si no tiene permisos
    });

// Configurar controladores con vistas
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Ejecutar DbInitializer para agregar los datos iniciales
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    DbInitializer.Initialize(context);  // Aquí se ejecuta el inicializador
}

// Configurar el middleware
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Agregar autenticación y autorización antes de las rutas
app.UseAuthentication();
app.UseAuthorization();

// Definir las rutas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
