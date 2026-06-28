using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using System.Text;
using System.Text.Json;
using TouchOfNature.Controllers;
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
        private MqttClientOptions? _options;
        private readonly IMapper _mapper;
        private readonly IHubContext<GreenhouseHub> _greenhouseHub;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MqttService> _logger;
        private readonly AutoControlSettings _autoSettings;
        private readonly ISensorStateService _sensorState;


        public MqttService(
            IServiceScopeFactory scopeFactory,
            IMapper mapper,
            IHubContext<GreenhouseHub> greenhouseHub,
            ILogger<MqttService> logger,
            IOptions<AutoControlSettings> autoSettings,
            ISensorStateService sensorState)
        {
            _scopeFactory = scopeFactory;
            _mapper = mapper;
            _greenhouseHub = greenhouseHub;
            _logger = logger;
            _autoSettings = autoSettings.Value;
            _sensorState = sensorState;
        }


        public async Task Connect()
        {
            var factory = new MqttFactory();
            _client = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithClientId("DotNetServer")
                .WithTcpServer("e8d5854690c14c6cb14aa85f6e47a48f.s1.eu.hivemq.cloud", 8883)
                .WithCredentials("SmartApp", "Sa12345678")
                .WithTlsOptions(o => { o.UseTls(); })
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
                    var sensorsOutput = new SensorsOutput
                    {
                        Timestamp = DateTime.Now,
                        SoilMoisture = root.TryGetProperty("soil", out var s) ? s.GetInt32() : -100,
                        LightDependentResistor = root.TryGetProperty("light", out var l) ? l.GetInt32() : -100,
                        Temperature = root.TryGetProperty("temp", out var t) ? (float)t.GetDouble() : -100.0f,
                        Humidity = root.TryGetProperty("humidity", out var h) ? (float)h.GetDouble() : -100.0f
                    };

                    using var scope = _scopeFactory.CreateScope();
                    var sensorsRepo = scope.ServiceProvider.GetRequiredService<ISensorsRepo>();
                    await sensorsRepo.AddSensorsOutput(sensorsOutput);

                    _logger.LogInformation("Data persisted. Soil: {Soil}, Light: {Light}, Temp: {Temp}, Hum: {Hum}",
                        sensorsOutput.SoilMoisture, sensorsOutput.LightDependentResistor,
                        sensorsOutput.Temperature, sensorsOutput.Humidity);

                    var dataDto = _mapper.Map<SensorsOutputUiDto>(sensorsOutput);
                    await _greenhouseHub.Clients.All.SendAsync("ReceiveSensorData", dataDto);

                    var autoRequest = new AutoControlRequestDto
                    {
                        SoilMoisture = sensorsOutput.SoilMoisture,
                        LightDependentResistor = sensorsOutput.LightDependentResistor,
                        Temperature = sensorsOutput.Temperature,
                        Humidity = sensorsOutput.Humidity
                    };
                    await EvaluateAutoControl(autoRequest);
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

                try { await _client.ConnectAsync(options); } 
                catch (Exception ex) { _logger.LogError(ex, "Reconnect failed"); }
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


        private bool _isLedOn;
        private bool _isFanOn;
        private bool _isPumpOn;

        public async Task EvaluateAutoControl(AutoControlRequestDto data)
        {
            if (!_autoSettings.Enabled)
            {
                _logger.LogInformation("Auto control is disabled, skipping evaluation.");

                if(_isLedOn)
                {
                    _isLedOn = false;
                    await SendCommand("LED_OFF");
                    await Task.Delay(100);
                }

                if(_isFanOn)
                {
                    _isFanOn = false;
                    await SendCommand("FAN_OFF");
                    await Task.Delay(100);
                }

                if (_isPumpOn)
                {
                    _isPumpOn = false;
                    await SendCommand("PUMP_OFF");
                    await Task.Delay(100);
                }
                return;
            }

            bool shouldLedOn = data.LightDependentResistor <= _autoSettings.LightThreshold;
            if (shouldLedOn != _isLedOn)
            {
                _isLedOn = shouldLedOn;

                await SendCommand(shouldLedOn ? "LED_ON" : "LED_OFF");
                await Task.Delay(100);
            }


            bool shouldFanOn = (data.Temperature >= _autoSettings.TempThreshold) ||
                (data.Humidity >= _autoSettings.HumidityThreshold);
            if (shouldFanOn != _isFanOn)
            {
                _isFanOn = shouldFanOn;

                await SendCommand(shouldFanOn ? "FAN_ON" : "FAN_OFF");
                await Task.Delay(100);
            }

            bool ShoudPumpOn = data.SoilMoisture <= _autoSettings.SoilMoistureThreshold;
            if(ShoudPumpOn != _isPumpOn)
            {
                _isPumpOn = ShoudPumpOn;

                await SendCommand(ShoudPumpOn ? "PUMP_ON" : "PUMP_OFF");
                await Task.Delay(100);
            }
        }
    }
}