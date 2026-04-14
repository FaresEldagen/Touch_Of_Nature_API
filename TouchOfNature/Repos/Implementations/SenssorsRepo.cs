using TouchOfNature.Data;
using TouchOfNature.Models;
using TouchOfNature.Repos.Interfaces;

namespace TouchOfNature.Repos.Implementations
{
    public class SenssorsRepo : ISenssorsRepo
    {
        private readonly AppDbContext Context;

        public SenssorsRepo(AppDbContext context)
        {
            Context = context;
        }

        public async Task AddSenssorsOutput(SenssorsOutput output)
        {
            Context.SenssorsOutputs.Add(output);
            await Context.SaveChangesAsync();
        }

    }
}
