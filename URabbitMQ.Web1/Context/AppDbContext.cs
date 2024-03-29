using Microsoft.EntityFrameworkCore;
using URabbitMQ.Web1.Models;

namespace URabbitMQ.Web1.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
    }
}
