using GOG_Backend.Models.Database;
using GOG_Backend.Services; // <-- AÑADIDO
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<MyDbContext>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- AÑADIMOS NUESTRO SERVICIO DE SESIÓN COMO SINGLETON ---
builder.Services.AddSingleton<SimpleSessionService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseStaticFiles();
app.MapControllers();

await InitDatabaseAsync(app.Services);
await app.RunAsync();

static async Task InitDatabaseAsync(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
    if (await dbContext.Database.EnsureCreatedAsync())
    {
        new Seeder(dbContext).Seed();
    }
}