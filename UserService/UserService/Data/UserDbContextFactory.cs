using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UserService.Data
{
    public class UserDbContextFactory : IDesignTimeDbContextFactory<UserDbContext>
    {
        public UserDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();

            optionsBuilder.UseSqlServer(
                "Server=localhost;Database=ShoeShop_Users;Integrated Security=True;TrustServerCertificate=True"
            );

            return new UserDbContext(optionsBuilder.Options);
        }
    }
}
