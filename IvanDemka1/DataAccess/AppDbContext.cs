using System.Data.Entity;
using IvanDemka1.Models;

namespace IvanDemka1.DataAccess
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() : base("name=DefaultConnection")
        {
            // Отключаем инициализатор, так как база уже существует (создана скриптом)
            Database.SetInitializer<AppDbContext>(null);
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Gender> Genders { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServicePhoto> ServicePhotos { get; set; }
        public DbSet<ClientService> ClientServices { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
