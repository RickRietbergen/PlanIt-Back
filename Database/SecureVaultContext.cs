using Microsoft.EntityFrameworkCore;
using SecureVault.Database.Entities;

namespace SecureVault.Database
{
    public class SecureVaultContext : DbContext
    {
        public SecureVaultContext(DbContextOptions<SecureVaultContext> options) : base(options)
        {
            
        }

        public DbSet<User> Users { get; set; }
    }
}
