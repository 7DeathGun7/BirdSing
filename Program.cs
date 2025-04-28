using BirdSing.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar DbContext con SQL Server y aumentar el timeout de comandos a 180 segundos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.CommandTimeout(180); // 3 minutos máximo por operación
        }
    )
);

// 2. Configurar autenticación por cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

// 3. Añadir soporte a controladores y vistas
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 4. Aplicar migraciones pendientes y sembrar datos iniciales
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        // Esto puede tardar si hay muchas migraciones o cambios de esquema grandes
        ctx.Database.Migrate();
    }
    catch (SqlException ex) when (ex.Number == -2)
    {
        // -2 = Command timeout
        // Aquí podrías registrar en log que no dio tiempo, pero seguir adelante
    }

    DbInitializer.Initialize(ctx);
}

// 5. Configurar pipeline de middlewares
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Importante: primero autenticación, luego autorización
app.UseAuthentication();
app.UseAuthorization();

// 6. Rutas MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
