using Microsoft.EntityFrameworkCore;
using PlanIt.Database.Entities;

namespace PlanIt.Database
{
    public class PlanIt : DbContext
    {
        public PlanIt(DbContextOptions<PlanIt> options) : base(options)
        {
            
        }

        public DbSet<User> Users { get; set; }
    }
}
