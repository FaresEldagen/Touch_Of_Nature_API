using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TouchOfNature.Services.Interfaces;

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
    }
}
