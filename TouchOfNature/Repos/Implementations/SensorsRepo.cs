using TouchOfNature.Data;
using TouchOfNature.Models;
using TouchOfNature.Repos.Interfaces;

namespace TouchOfNature.Repos.Implementations
{
    public class SensorsRepo : ISensorsRepo
    {
        private readonly AppDbContext Context;

        public SensorsRepo(AppDbContext context)
        {
            Context = context;
        }

        public async Task AddSensorsOutput(SensorsOutput output)
        {
            Context.SensorsOutputs.Add(output);
            await Context.SaveChangesAsync();
        }

    }
}
