using Microsoft.EntityFrameworkCore;
using TouchOfNature.Data;
using TouchOfNature.Repos.Implementations;
using TouchOfNature.Repos.Interfaces;
using TouchOfNature.Services.Implementations;
using TouchOfNature.Services.Interfaces;

namespace TouchOfNature
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // ================= Database =================
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("Local")));

            // ================= DI =================
            builder.Services.AddScoped<ISenssorsRepo, SenssorsRepo>();

            // ================= MQTT =================
            builder.Services.AddSingleton<IMqttService, MqttService>();
            builder.Services.AddHostedService<MqttBackgroundService>();



            var app = builder.Build();

            // Configure the HTTP request pipeline
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