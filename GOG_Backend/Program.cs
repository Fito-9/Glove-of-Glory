using GOG_Backend;
using GOG_Backend.Models.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using GOG_Backend.WebSockets;
using Microsoft.AspNetCore.WebSockets;
using StrategoBackend.WebSockets;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<MyDbContext>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<WebSocketNetwork>();
builder.Services.AddTransient<MatchmakingWebSocketMiddleware>();

builder.Services.AddSingleton(provider =>
{
    Settings settings = builder.Configuration.GetSection(Settings.SECTION_NAME).Get<Settings>();
    return new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.JwtKey))
    };
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddAuthentication().AddJwtBearer();

static async Task InitDatabaseAsync(IServiceProvider serviceProvider)
{
    using IServiceScope scope = serviceProvider.CreateScope();
    using MyDbContext dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();

    if (dbContext.Database.EnsureCreated())
    {
        Seeder seeder = new Seeder(dbContext);
        seeder.Seed();
    }
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.UseWebSockets();
app.UseMiddleware<MatchmakingWebSocketMiddleware>();


app.MapControllers();

await InitDatabaseAsync(app.Services);
await app.RunAsync();
