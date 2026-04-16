using TouchOfNature.Models;
using Microsoft.EntityFrameworkCore;
using TouchOfNature.Data;
using TouchOfNature.Hubs;
using TouchOfNature.Repos.Implementations;
using TouchOfNature.Repos.Interfaces;
using TouchOfNature.Services.Implementations;
using TouchOfNature.Services.Interfaces;

namespace TouchOfNature
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // ================= Database =================
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("Server")));
                //options.UseSqlServer(builder.Configuration.GetConnectionString("Local")));

            // ================= DI =================
            builder.Services.AddScoped<ISensorsRepo, SensorsRepo>();

            // ================= MQTT =================
            builder.Services.AddSingleton<IMqttService, MqttService>();
            builder.Services.AddHostedService<MqttBackgroundService>();

            // ================= SignalR =================
            builder.Services.AddSignalR();

            // ================= AutoMapper =================
            builder.Services.AddAutoMapper(typeof(Program));
            
            // ================= CORS =================
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyHeader()
                          .AllowAnyMethod()
                          .SetIsOriginAllowed(_ => true)
                          .AllowCredentials();
                });
            });

            // ================= Auto Control Settings =================
            builder.Services.Configure<AutoControlSettings>(
                builder.Configuration.GetSection("AutoControlSettings"));

            // ================= Sensor State =================
            builder.Services.AddSingleton<ISensorStateService, SensorStateService>();

            builder.Services.Configure<AutoControlSettings>(
                builder.Configuration.GetSection("AutoControlSettings")
            );
            var app = builder.Build();

            // Configure the HTTP request pipeline
            //if (app.Environment.IsDevelopment())
            //{
            //    app.UseSwagger();
            //    app.UseSwaggerUI();
            //}
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseCors("AllowAll");
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.MapHub<GreenhouseHub>("/hubs/greenhouse");

            app.Run();
        }
    }
}