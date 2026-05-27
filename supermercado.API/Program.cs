using supermercado.API.Data;
using supermercado.API.Services;
using supermercado.API.BackgroundServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text.Json.Serialization;

namespace supermercado.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler =
                        ReferenceHandler.IgnoreCycles;
                });

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Azure Queue Storage: se usa para enviar notificaciones asincronas de pedidos.
            builder.Services.AddSingleton<QueueService>();

            // Servicio de correo para el worker que procesa la cola.
            builder.Services.AddTransient<EmailService>();

            // Worker en segundo plano para consumir mensajes de la cola.
            builder.Services.AddHostedService<QueueConsumerWorker>();

            // Health check para Docker, Postman, K6 y evidencias de testing.
            builder.Services.AddHealthChecks();

            // Swagger documentado para la entrega de Hector Jesus.
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Supermercado Online API",
                    Version = "v1",
                    Description = "API principal para gestion de productos, clientes, ventas, carrito y notificaciones asincronas mediante Azure Queue Storage. Incluye endpoints de prueba para UI, Postman, xUnit y K6.",
                    Contact = new OpenApiContact
                    {
                        Name = "Hector Jesus - UI, Testing y Entrega"
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

            var swaggerEnabled = app.Environment.IsDevelopment() ||
                app.Configuration.GetValue("Swagger:Enabled", true);

            if (swaggerEnabled)
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Supermercado Online API v1");
                    options.DocumentTitle = "Swagger - Supermercado Online";
                    options.RoutePrefix = "swagger";
                });
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();
            app.MapHealthChecks("/health");
            app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

            app.Run();
        }
    }
}
