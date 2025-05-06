using Lib.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Lib.Persistence;

public class RoleInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

        string[] roleNames = { "User", "SuperUser" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>
                {
                    Id = Guid.NewGuid(),
                    Name = roleName,
                    NormalizedName = roleName.ToUpper()
                });
            }
        }

        var superUser = new User
        {
            Id = Guid.NewGuid(),
            UserName = "admin",
            NormalizedUserName = "ADMIN",
            Email = "admin@gmail.com",
            NormalizedEmail = "ADMIN@GMAIL.COM"
        };

        string superUserPassword = "Admin@123";
        var existingSuperUser = await userManager.FindByNameAsync(superUser.UserName);
        if (existingSuperUser == null)
        {
            var createSuperUser = await userManager.CreateAsync(superUser, superUserPassword);
            if (createSuperUser.Succeeded)
            {
                await userManager.AddToRoleAsync(superUser, "SuperUser");
            }
        }
    }
}
