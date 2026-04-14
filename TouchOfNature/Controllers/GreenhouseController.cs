using Microsoft.AspNetCore.Mvc;
using TouchOfNature.Services.Interfaces;
using TouchOfNature.Models;

namespace TouchOfNature.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class GreenhouseController : ControllerBase
    {
        private readonly IMqttService _mqtt;

        public GreenhouseController(IMqttService mqtt)
        {
            _mqtt = mqtt;
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
        [HttpPost("light/on")]
        public async Task<IActionResult> LightOn()
        {
            await _mqtt.SendCommand("LIGHT_ON");
            return Ok("Light ON sent");
        }

        [HttpPost("light/off")]
        public async Task<IActionResult> LightOff()
        {
            await _mqtt.SendCommand("LIGHT_OFF");
            return Ok("Light OFF sent");
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
        [HttpPost("auto")]
        public async Task<IActionResult> AutoControl([FromBody] AutoControlRequest request)
        {
            await _mqtt.EvaluateAutoControl(request);
            return Ok("Auto control evaluated");
        }
    }

    public class AutoControlRequest
    {
        public int SoilMoisture { get; set; }
        public int LightDependentResistor { get; set; }
        public float Temperature { get; set; }
        public float Humidity { get; set; }
    }
}