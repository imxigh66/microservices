using IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace IdentityService.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvidert)
        {
            var roleManager = serviceProvidert.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvidert.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roleNames = {AppRoles.Admin, AppRoles.User };

            foreach (var roleName in roleNames)
            {
                var roleExist =await  roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await  roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Создание админа по умолчанию
            var adminEmail = "admin@shoeshop.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(admin, "Admin@123456");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, AppRoles.Admin);
                }
            }
        }
    }
}
