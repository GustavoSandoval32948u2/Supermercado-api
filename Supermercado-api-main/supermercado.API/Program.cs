using supermercado.API.Data;
using supermercado.API.Services;
using supermercado.API.BackgroundServices;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;


namespace supermercado.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler =
                        ReferenceHandler.IgnoreCycles;
                });
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // azure queue storage - singleton porque QueueClient es thread-safe
            builder.Services.AddSingleton<QueueService>();

            // email service
            builder.Services.AddTransient<EmailService>();

            // worker que corre en background revisando la cola cada 10 segundos
            builder.Services.AddHostedService<QueueConsumerWorker>();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
