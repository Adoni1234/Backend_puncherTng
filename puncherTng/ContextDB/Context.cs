using Microsoft.EntityFrameworkCore;
using puncherTng.Models;

namespace puncherTng.ContextDB
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options) { }

        public DbSet<Usuarios> usuarios { get; set; }
        public DbSet<Agentes> agentes { get; set; }
        public DbSet<AccessCode> accessCodes { get; set; }
        public DbSet<History> histories { get; set; }
        public DbSet<Access> accesses { get; set; }
        public DbSet<Companies> companie { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Access>()
                .Property(a => a.Id)
                .ValueGeneratedOnAdd();
        }
    }
}
