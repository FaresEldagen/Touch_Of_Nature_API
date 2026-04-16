using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TouchOfNature.DTOs;
using TouchOfNature.Models;
using TouchOfNature.Services.Interfaces;

namespace TouchOfNature.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class GreenhouseController : ControllerBase
    {
        private readonly IMqttService _mqtt;
        private readonly ISensorStateService _sensorState;
        private readonly AutoControlSettings _settings;

        public GreenhouseController(IMqttService mqtt, ISensorStateService sensorState, IOptions<AutoControlSettings> options)
        {
            _mqtt = mqtt;
            _sensorState = sensorState;
            _settings = options.Value;
        }

        // ===== FAN =====
        [HttpPost("fan/on")]
        public async Task<IActionResult> FanOn()
        {
            await _mqtt.SendCommand("FAN_ON");
            return Ok("Fan ON sent");
        }

        [HttpPost("fan/off")]
        public async Task<IActionResult> FanOff()
        {
            await _mqtt.SendCommand("FAN_OFF");
            return Ok("Fan OFF sent");
        }

        // ===== LIGHT =====
        [HttpPost("led/on")]
        public async Task<IActionResult> LightOn()
        {
            await _mqtt.SendCommand("LED_ON");
            return Ok("LED ON sent");
        }

        [HttpPost("led/off")]
        public async Task<IActionResult> LightOff()
        {
            await _mqtt.SendCommand("LED_OFF");
            return Ok("LED OFF sent");
        }

        // ===== PUMP =====
        [HttpPost("pump/on")]
        public async Task<IActionResult> PumpOn()
        {
            await _mqtt.SendCommand("PUMP_ON");
            return Ok("Pump ON sent");
        }

        [HttpPost("pump/off")]
        public async Task<IActionResult> PumpOff()
        {
            await _mqtt.SendCommand("PUMP_OFF");
            return Ok("Pump OFF sent");
        }

        // ===== AUTO CONTROL =====
        [HttpGet("auto-control-get")]
        public IActionResult GetAutoControlValues()
        {
            return Ok(_settings);
        }

        [HttpPost("auto-control-enable")]
        public IActionResult SetAutoControlState(bool Enable)
        {
            _settings.Enabled = Enable;
            return Ok();
        }

        [HttpPost("auto-control-update")]
        public IActionResult UpdateAutoControlThresholds([FromBody] AutoControlRequestDto dto)
        {
            _settings.LightThreshold = dto.LightDependentResistor;
            _settings.HumidityThreshold = dto.Humidity;
            _settings.TempThreshold = dto.Temperature;
            _settings.SoilMoistureThreshold = dto.SoilMoisture;
            return Ok();
        }
    }
}