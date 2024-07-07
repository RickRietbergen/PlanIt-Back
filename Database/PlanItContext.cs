using Microsoft.EntityFrameworkCore;
using PlanIt.Database.Entities;

namespace PlanIt.Database
{
    public class PlanItContext : DbContext
    {
        public PlanItContext(DbContextOptions<PlanItContext> options) : base(options)
        {
            
        }

        public DbSet<User> Users { get; set; }
    }
}
