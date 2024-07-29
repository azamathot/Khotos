using Microsoft.AspNetCore.Identity;

namespace WebUI_Wasm.Data
{
    public class DbInitializer
    {
        public static async Task SeedAsync(IApplicationBuilder applicationBuilder, ConfigurationManager config)
        {
            var scope = applicationBuilder.ApplicationServices.CreateScope();
            ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            if (context != null && !context.Roles.Any())
            {
                context.Roles.AddRange(
                    new IdentityRole { Name = RoleConsts.Admin, NormalizedName = RoleConsts.Admin.ToUpper() },
                    new IdentityRole { Name = RoleConsts.Client, NormalizedName = RoleConsts.Client.ToUpper() },
                    new IdentityRole { Name = RoleConsts.Moderator, NormalizedName = RoleConsts.Moderator.ToUpper() }
                );
                context.SaveChanges();

                string password = config.GetSection("AdminUser")["Password"];
                string adminEmail = config.GetSection("AdminUser")["AdminEmail"];
                UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                ApplicationUser? admin = context.Users.FirstOrDefault(x => x.Email == adminEmail);
                if (admin == null)
                {
                    admin = new ApplicationUser { Email = adminEmail, UserName = adminEmail };
                    IdentityResult result = await userManager.CreateAsync(admin, password);
                    if (result.Succeeded)
                    {
                        var code = await userManager.GenerateEmailConfirmationTokenAsync(admin);
                        var confirmResult = await userManager.ConfirmEmailAsync(admin, code);
                        //if (confirmResult.Succeeded)
                        //    await userManager.AddToRoleAsync(admin, RoleConsts.Admin);
                    }
                }
                await userManager.AddToRoleAsync(admin, RoleConsts.Admin);
            }
        }
    }
}
