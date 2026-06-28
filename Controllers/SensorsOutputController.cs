using Microsoft.AspNetCore.Mvc;
using TouchOfNature.Models;
using TouchOfNature.Repos.Interfaces;

namespace TouchOfNature.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SensorsOutputController : ControllerBase
    {
        private readonly ISensorsRepo _sensorsRepo;

        public SensorsOutputController(ISensorsRepo sensorsRepo)
        {
            _sensorsRepo = sensorsRepo;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SensorsOutput>>> GetAllRows()
        {
            var results = await _sensorsRepo.GetAllSensorsOutputs();
            return Ok(results);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAllRows()
        {
            await _sensorsRepo.DeleteAllSensorsOutputs();
            return Ok("All sensor output rows have been deleted.");
        }
    }
}
