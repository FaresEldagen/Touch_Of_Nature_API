using Microsoft.AspNetCore.Mvc;
using TouchOfNature.Services.Interfaces;
using TouchOfNature.Models;
using TouchOfNature.DTOs;

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
        [HttpGet("auto/get")]
        public async Task<IActionResult> GetAutoControlValues()
        {
            //await _mqtt.EvaluateAutoControl(request);
            return Ok("Auto control evaluated");
        }

        [HttpPost("auto/on")]
        public async Task<IActionResult> AutoControlEnable([FromBody] AutoControlRequestDto request)
        {
            await _mqtt.EvaluateAutoControl(request);
            return Ok("Auto control evaluated");
        }

        [HttpPost("auto/off")]
        public async Task<IActionResult> AutoControlDisable()
        {
            //await _mqtt.EvaluateAutoControl(request);
            return Ok("Auto control evaluated");
        }
    }
}