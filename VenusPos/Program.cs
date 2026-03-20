using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using VenusPos.Application.Interfaces.Repositories;
using VenusPos.Application.Interfaces.Services;
using VenusPos.Application.Services.Cliente;   
using VenusPos.Application.Services.Empleado;
using VenusPos.Application.Services.Mascota;
using VenusPos.Application.Services.Servicio;
using VenusPos.Infrastructure.Data;
using VenusPos.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ── Base de datos ──────────────────────────────────────────────────────────
builder.Services.AddDbContext<VenusPosDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── CORS ───────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// ── JWT ────────────────────────────────────────────────────────────────────
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

// ── Inyección de dependencias ──────────────────────────────────────────────
builder.Services.AddScoped<IEmpleadoRepository, EmpleadoRepository>();
builder.Services.AddScoped<IEmpleadoService, EmpleadoService>();

builder.Services.AddScoped<IClienteRepository, ClienteRepository>(); 
builder.Services.AddScoped<IClienteService, ClienteService>();    

builder.Services.AddScoped<IServicioRepository, ServicioRepository>();
builder.Services.AddScoped<IServicioService, ServicioService>();

builder.Services.AddScoped<IMascotaRepository, MascotaRepository>();
builder.Services.AddScoped<IMascotaService, MascotaService>();
// ── Controladores y OpenAPI ────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// ── Pipeline ───────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();