using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using supermercado.API.Data;
using supermercado.API.Services;
using supermercado.API.Workers;

namespace supermercado.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // =========================
            // CONTROLLERS
            // =========================
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler =
                        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                });

            // =========================
            // DB
            // =========================
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // =========================
            // SERVICES
            // =========================
            builder.Services.AddSingleton<QueueService>();
            builder.Services.AddTransient<EmailService>();

            // Worker (solo una vez)
            builder.Services.AddHostedService<PedidoWorker>();

            // =========================
            // HEALTH CHECK
            // =========================
            builder.Services.AddHealthChecks();

            // =========================
            // CORS (IMPORTANTE PARA UI)
            // =========================
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            // =========================
            // SWAGGER
            // =========================
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Supermercado Online API",
                    Version = "v1",
                    Description = "API para productos, clientes, ventas, carrito y notificaciones async.",
                    Contact = new OpenApiContact
                    {
                        Name = "Equipo Supermercado"
                    }
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            });

            var app = builder.Build();

            // =========================
            // PIPELINE
            // =========================

            // Swagger
            app.UseSwagger();
            app.UseSwaggerUI();

            // ❌ IMPORTANTE: NO HTTPS REDIRECTION (ROMPE CORS + UI)
            // app.UseHttpsRedirection();

            // CORS (DEBE IR ANTES DE AUTH)
            app.UseCors("AllowAll");

            app.UseAuthorization();

            app.MapControllers();
            app.MapHealthChecks("/health");

            app.MapGet("/", () => Results.Redirect("/swagger"))
                .ExcludeFromDescription();

            app.Run();
        }
    }
}