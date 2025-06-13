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

// Registro de servicios 
builder.Services.AddScoped<MyDbContext>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// El WebSocketNetwork es único para toda la app, para que todos los jugadores estén en el mismo sitio.
builder.Services.AddSingleton<WebSocketNetwork>();
builder.Services.AddTransient<MatchmakingWebSocketMiddleware>();
builder.Services.AddScoped<FriendshipService>();

// Define cómo se van a validar los tokens de acceso de los usuarios.
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

// Uso de Cors
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddAuthentication().AddJwtBearer();

// Función para crear la base de datos y meterle datos de prueba si es la primera vez que se ejecuta.
static async Task InitDatabaseAsync(IServiceProvider serviceProvider)
{
    using IServiceScope scope = serviceProvider.CreateScope();
    using MyDbContext dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();

    if (dbContext.Database.EnsureCreated())
    {
        new Seeder(dbContext).Seed();
    }
}

var app = builder.Build();

//  Peticiones HTTP 

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseStaticFiles(); // Para poder servir las imágenes de perfil.

app.UseAuthentication();
app.UseAuthorization();

// Activamos los WebSockets.
app.UseWebSockets();
app.UseMiddleware<MatchmakingWebSocketMiddleware>();

app.MapControllers();

// Preparamos la base de datos antes de arrancar.
await InitDatabaseAsync(app.Services);
await app.RunAsync();