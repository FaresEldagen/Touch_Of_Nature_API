using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using MQTTnet;
using MQTTnet.Client;
using System.Text;
using System.Text.Json;
using TouchOfNature.DTOs;
using TouchOfNature.Hubs;
using TouchOfNature.Models;
using TouchOfNature.Repos.Interfaces;
using TouchOfNature.Services.Interfaces;

namespace TouchOfNature.Services.Implementations
{
    public class MqttService : IMqttService
    {
        private IMqttClient? _client;
        private readonly IMapper _mapper;
        private readonly IHubContext<GreenhouseHub> _greenhouseHub;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MqttService> _logger;

        public MqttService(
            IServiceScopeFactory scopeFactory, 
            IMapper mapper, 
            IHubContext<GreenhouseHub> greenhouseHub,
            ILogger<MqttService> logger)
        {
            _scopeFactory = scopeFactory;
            _mapper = mapper;
            _greenhouseHub = greenhouseHub;
            _logger = logger;
        }


        public async Task Connect()
        {
            var factory = new MqttFactory();
            _client = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithClientId("DotNetServer")
                .WithTcpServer("192.168.1.3", 1883)
                .WithCleanSession()
                .Build();

            _client.ApplicationMessageReceivedAsync += async e =>
            {
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
                _logger.LogInformation("MQTT Received: {Payload}", payload);

                try
                {
                    using var doc = JsonDocument.Parse(payload);
                    var root = doc.RootElement;

                    // Safe extraction with fallback values
                    var senssorsOutput = new SenssorsOutput
                    {
                        Timestamp = DateTime.Now,
                        SoilMoisture = root.TryGetProperty("soil", out var s) ? s.GetInt32() : 0,
                        LightDependentResistor = root.TryGetProperty("light", out var l) ? l.GetInt32() : 0,
                        Temperature = root.TryGetProperty("temp", out var t) ? (float)t.GetDouble() : 0.0f,
                        Humidity = root.TryGetProperty("humidity", out var h) ? (float)h.GetDouble() : 0.0f
                    };

                    using var scope = _scopeFactory.CreateScope();
                    var senssorsRepo = scope.ServiceProvider.GetRequiredService<ISenssorsRepo>();
                    await senssorsRepo.AddSenssorsOutput(senssorsOutput);

                    _logger.LogInformation("Data persisted. Soil: {Soil}, Light: {Light}, Temp: {Temp}, Hum: {Hum}", 
                        senssorsOutput.SoilMoisture, senssorsOutput.LightDependentResistor, 
                        senssorsOutput.Temperature, senssorsOutput.Humidity);

                    var dataDto = _mapper.Map<SenssorsOutputUiDto>(senssorsOutput);
                    await _greenhouseHub.Clients.All.SendAsync("ReceiveSensorData", dataDto);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing MQTT message");
                }
            };

            // Basic reconnection logic
            _client.DisconnectedAsync += async e =>
            {
                _logger.LogWarning("MQTT Disconnected. Retrying in 5 seconds...");
                await Task.Delay(TimeSpan.FromSeconds(5));
                try { await _client.ConnectAsync(options); } catch { }
            };

            await _client.ConnectAsync(options);
            await _client.SubscribeAsync("greenhouse/sensors");
            _logger.LogInformation("MQTT Connected + Subscribed to greenhouse/sensors");
        }

        public async Task SendCommand(string command)
        {
            if (_client == null || !_client.IsConnected)
            {
                _logger.LogError("Cannot send command: MQTT client not connected.");
                return;
            }

            var message = new MqttApplicationMessageBuilder()
                .WithTopic("greenhouse/commands")
                .WithPayload(command)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            await _client.PublishAsync(message);
        }
    }
}
