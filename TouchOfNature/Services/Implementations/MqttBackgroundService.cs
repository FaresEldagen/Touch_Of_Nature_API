using Microsoft.Extensions.Hosting;
using TouchOfNature.Services.Interfaces;

namespace TouchOfNature.Services.Implementations
{
    public class MqttBackgroundService : BackgroundService
    {
        private readonly IMqttService _mqtt;

        public MqttBackgroundService(IMqttService mqtt)
        {
            _mqtt = mqtt;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _mqtt.Connect();

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
