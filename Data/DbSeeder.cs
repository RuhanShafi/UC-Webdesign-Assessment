// CREATING DUMMY ADMIN ACCOUNT

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public static class DbSeeder
{
    public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
    {
        // Get the required services
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // 1. Ensure the "Admin" Role exists
        string adminRoleName = "Admin";
        if (await roleManager.FindByNameAsync(adminRoleName) == null)
        {
            await roleManager.CreateAsync(new IdentityRole(adminRoleName));
        }

        // 2. Ensure the Admin User exists and has the role
        string adminEmail = "admin@asp.net"; //  
        string adminPassword = "AdminPassword123!"; 

        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            // Create the user
            var adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            var result = await userManager.CreateAsync(adminUser, adminPassword);

            if (result.Succeeded)
            {
                // Assign the user to the "Admin" role
                await userManager.AddToRoleAsync(adminUser, adminRoleName);
            }
        }
    }
}