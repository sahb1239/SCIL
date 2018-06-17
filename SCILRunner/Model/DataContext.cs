using Microsoft.EntityFrameworkCore;

namespace SCILRunner.Model
{
    public class DataContext : DbContext
    {
        protected DataContext()
        {
        }

        public DataContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Scan> Scans { get; set; }
    }
}