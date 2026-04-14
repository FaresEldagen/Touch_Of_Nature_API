using MQTTnet;
using MQTTnet.Client;
using System.Text;
using TouchOfNature.Models;
using TouchOfNature.Repos.Interfaces;
using TouchOfNature.Services.Interfaces;

namespace TouchOfNature.Services.Implementations
{
    public class MqttService : IMqttService
    {
        private IMqttClient? _client;
        private readonly IServiceScopeFactory _scopeFactory;

        public MqttService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }


        public async Task Connect()
        {
            var factory = new MqttFactory();
            _client = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithClientId("DotNetServer")
                .WithTcpServer("192.168.1.3", 1883)
                .Build();

            _client.ApplicationMessageReceivedAsync += async e =>
            {
                var topic = e.ApplicationMessage.Topic;
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);

                Console.WriteLine($"Topic: {topic}");
                Console.WriteLine($"Data: {payload}");

                try
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(payload);
                    var root = doc.RootElement;

                    SenssorsOutput senssorsOutput = new SenssorsOutput
                    {
                        Timestamp = DateTime.Now,
                        SoilMoisture = root.GetProperty("soil").GetInt32(),
                        LightDependentResistor = root.GetProperty("light").GetInt32(),
                        Temperature = (float)root.GetProperty("temp").GetDouble(),
                        Humidity = (float)root.GetProperty("humidity").GetDouble()
                    };

                    using var scope = _scopeFactory.CreateScope();
                    var senssorsRepo = scope.ServiceProvider.GetRequiredService<ISenssorsRepo>();
                    await senssorsRepo.AddSenssorsOutput(senssorsOutput);

                    Console.WriteLine("Sensor data saved to database.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing MQTT message: {ex.Message}");
                }
            };

            await _client.ConnectAsync(options);

            await _client.SubscribeAsync("greenhouse/sensors");

            Console.WriteLine("MQTT Connected + Subscribed");
        }

        public async Task SendCommand(string command)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic("greenhouse/commands")
                .WithPayload(command)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag(false)
                .Build();

            await _client!.PublishAsync(message);
        }
    }
}
