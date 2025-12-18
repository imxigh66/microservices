using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // Укажите вашу строку подключения
            optionsBuilder.UseSqlServer(
                "Server=localhost;Database=ShoeShop_Identity;Integrated Security=True;TrustServerCertificate=True"
            );

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
