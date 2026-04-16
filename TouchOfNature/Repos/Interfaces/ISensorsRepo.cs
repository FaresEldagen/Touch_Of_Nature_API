using TouchOfNature.Models;

namespace TouchOfNature.Repos.Interfaces
{
    public interface ISensorsRepo
    {
        public Task AddSensorsOutput(SensorsOutput output);
        public Task<List<SensorsOutput>> GetAllSensorsOutputs();
        public Task DeleteAllSensorsOutputs();
    }
}
