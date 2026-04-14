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

public class MqttService : IMqttService
{
    private IMqttClient? _client;
    private MqttClientOptions? _options; // ✅ field مش local variable
    private readonly IMapper _mapper;
    private readonly IHubContext<GreenhouseHub> _greenhouseHub;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MqttService> _logger;
    private readonly AutoControlSettings _autoSettings;

    public MqttService(
        IServiceScopeFactory scopeFactory,
        IMapper mapper,
        IHubContext<GreenhouseHub> greenhouseHub,
        ILogger<MqttService> logger,
        IOptions<AutoControlSettings> autoSettings)
    {
        _scopeFactory = scopeFactory;
        _mapper = mapper;
        _greenhouseHub = greenhouseHub;
        _logger = logger;
        _autoSettings = autoSettings.Value;
    }

    public async Task Connect()
    {
        var factory = new MqttFactory();
        _client = factory.CreateMqttClient();

        _options = new MqttClientOptionsBuilder()
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

                var sensorsOutput = new SenssorsOutput
                {
                    Timestamp = DateTime.Now,
                    SoilMoisture = root.TryGetProperty("soil", out var s) ? s.GetInt32() : 0,
                    LightDependentResistor = root.TryGetProperty("light", out var l) ? l.GetInt32() : 0,
                    Temperature = root.TryGetProperty("temp", out var t) ? (float)t.GetDouble() : 0.0f,
                    Humidity = root.TryGetProperty("humidity", out var h) ? (float)h.GetDouble() : 0.0f
                };

                using var scope = _scopeFactory.CreateScope();
                var sensorsRepo = scope.ServiceProvider.GetRequiredService<ISenssorsRepo>();
                await sensorsRepo.AddSenssorsOutput(sensorsOutput);

                var dataDto = _mapper.Map<SenssorsOutputUiDto>(sensorsOutput);
                await _greenhouseHub.Clients.All.SendAsync("ReceiveSensorData", dataDto);

                // ✅ Auto evaluate بعد كل reading
                var autoRequest = new AutoControlRequest
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

        _client.DisconnectedAsync += async e =>
        {
            _logger.LogWarning("MQTT Disconnected. Retrying in 5 seconds...");
            await Task.Delay(TimeSpan.FromSeconds(5));
            try { await _client.ConnectAsync(_options!); }
            catch (Exception ex) { _logger.LogError(ex, "Reconnect failed"); } // ✅ log الـ error
        };

        await _client.ConnectAsync(_options);
        await _client.SubscribeAsync("greenhouse/sensors");
        _logger.LogInformation("MQTT Connected + Subscribed");
    }

    public async Task EvaluateAutoControl(AutoControlRequest data)
    {
        // 💡 LIGHT — لو الإضاءة قلت عن الـ threshold
        if (data.LightDependentResistor < _autoSettings.LightThreshold)
        {
            _logger.LogInformation("Auto: Light low ({Val}), sending LIGHT_ON", data.LightDependentResistor);
            await SendCommand("LIGHT_ON");
        }
        else
        {
            await SendCommand("LIGHT_OFF");
        }

        // 🌡️ FAN — لو الحرارة أو رطوبة الهواء عالية
        if (data.Temperature > _autoSettings.TempThreshold ||
            data.Humidity > _autoSettings.HumidityThreshold)
        {
            _logger.LogInformation("Auto: Temp={T} Hum={H}, sending FAN_ON",
                data.Temperature, data.Humidity);
            await SendCommand("FAN_ON");
        }
        else
        {
            await SendCommand("FAN_OFF");
        }

        // 💧 PUMP — لو رطوبة التربة وقعت
        if (data.SoilMoisture < _autoSettings.SoilMoistureThreshold)
        {
            _logger.LogInformation("Auto: Soil dry ({Val}), sending PUMP_ON", data.SoilMoisture);
            await SendCommand("PUMP_ON");
        }
        else
        {
            await SendCommand("PUMP_OFF");
        }
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
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        await _client.PublishAsync(message);
    }
}