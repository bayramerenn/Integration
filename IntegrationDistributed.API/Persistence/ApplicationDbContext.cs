using Microsoft.EntityFrameworkCore;

namespace IntegrationDistributed.API.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
           : base(options)
        {
        }

        public DbSet<Item> Items { get; set; }
    }
}