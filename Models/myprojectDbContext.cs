using Microsoft.EntityFrameworkCore;

namespace myproject.Models
{
    public class myprojectDbContext : DbContext
    {
          public myprojectDbContext(DbContextOptions<myprojectDbContext> options) : base(options)
        {

        }
        public DbSet<Data> data { get; set; }
        public DbSet<DB> db { get; set; }
    }
}