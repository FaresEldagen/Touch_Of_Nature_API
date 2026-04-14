using Microsoft.EntityFrameworkCore;
using TouchOfNature.Models;

namespace TouchOfNature.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options) { }


        public virtual DbSet<SenssorsOutput> SenssorsOutputs => Set<SenssorsOutput>();
    }
}
