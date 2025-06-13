using GOG_Backend;
using GOG_Backend.Models.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using GOG_Backend.WebSockets;
using Microsoft.AspNetCore.WebSockets;
using StrategoBackend.WebSockets;
using GOG_Backend.Services;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// --- Registro de servicios ---

// 1. Configuración del DbContext para que sea flexible
builder.Services.AddDbContext<MyDbContext>(options =>
{
    // Leemos la cadena de conexión de MySQL desde appsettings.json
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    // Usamos el "proveedor" de MySQL para conectar a la base de datos.
    // ServerVersion.AutoDetect(connectionString) detecta automáticamente la versión del servidor MySQL.
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<WebSocketNetwork>();
builder.Services.AddTransient<MatchmakingWebSocketMiddleware>();
builder.Services.AddScoped<FriendshipService>();

// --- Configuración de Seguridad (JWT) ---
builder.Services.AddSingleton(provider =>
{
    Settings settings = builder.Configuration.GetSection(Settings.SECTION_NAME).Get<Settings>();
    return new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.JwtKey)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// --- CORS ---
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddAuthentication().AddJwtBearer();

// --- Función de inicialización de la Base de Datos ---
static async Task InitDatabaseAsync(IServiceProvider serviceProvider)
{
    using IServiceScope scope = serviceProvider.CreateScope();
    using MyDbContext dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();

    // MigrateAsync() aplicará las migraciones pendientes.
    // Esto crea las tablas en la base de datos de MySQL la primera vez que se ejecute en el servidor.
    await dbContext.Database.MigrateAsync();

    // El Seeder para meter datos iniciales (como el usuario admin).
    if (!await dbContext.Users.AnyAsync())
    {
        new Seeder(dbContext).Seed();
    }
}

var app = builder.Build();

// --- Pipeline de Peticiones HTTP ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseWebSockets();
app.UseMiddleware<MatchmakingWebSocketMiddleware>();

app.MapControllers();

// Preparamos la base de datos antes de arrancar.
await InitDatabaseAsync(app.Services);
await app.RunAsync();