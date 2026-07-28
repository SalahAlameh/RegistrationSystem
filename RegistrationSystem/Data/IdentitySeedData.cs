using Microsoft.AspNetCore.Identity;

namespace RegistrationSystem.Data
{
    public static class IdentitySeedData
    {
        public static async Task InitializeAsync( // async means the method can use await, task means the method performs asynchronous work buyt does not return a value
            IServiceProvider services, // receives the app's service provider, services like UserManager, RoleManager and ApplicationDbContext
            IConfiguration configuration) // receives the app's configuration, which is used to read the admin email from appsettings.json
        {
            // CreateScope gives us access to scoped Identity services during application startup.
            using var scope = services.CreateScope(); // creates a DI scope that allows us to resolve scoped services like UserManager and RoleManager. The using statement ensures that the scope is disposed of when we're done with it.

            var roleManager = scope.ServiceProvider 
                .GetRequiredService<RoleManager<IdentityRole>>(); // starts retrieving the Identity role manager from the new scope

            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<IdentityUser>>(); // starts retrieving the Identity user manager from the new scope

            // these are the two roles currently needed by the application.
            string[] requiredRoles = { "Admin", "User" };

            foreach (var roleName in requiredRoles) // roleName like "Admin" or "User"
            {
                // roleExistsAsync prevents duplicate roles whenever the application restarts.
                if (!await roleManager.RoleExistsAsync(roleName)) // checks whether the current role does not exist
                { //await waits for the database operation to finish before continuing. This is important because we don't want to try to create a role that already exists.
                    var roleResult = await roleManager.CreateAsync( // asks Identity to create the role and waits for the operation to finish
                        new IdentityRole(roleName)); // creates new IdentityRole object using the current role name

                    if (!roleResult.Succeeded) // checks whether Identity failed to create the role
                    {
                        var errors = string.Join( // starts combining the error descriptions into one string
                            ", ", // errors will be separated by a comma and a space
                            roleResult.Errors.Select(error => error.Description)); // gets description from every Identity error

                        throw new InvalidOperationException( // stops application startup by throwing an exception
                            $"Could not create the {roleName} role: {errors}"); // $: enables string interpolation
                    }
                }
            }

            // the email comes from appsettings.json instead of being permanently
            // hard-coded inside this class.
            var adminEmail = configuration["AdminAccount:Email"];

            if (string.IsNullOrWhiteSpace(adminEmail)) // checks whether the admin email is null, empty, or only spaces
            {
                return;
            }

            // the initial administrator must already have a registered Identity account.
            // the account must already exist in AspNetUsers
            var adminUser = await userManager.FindByEmailAsync(adminEmail); // searches AspNetUsers for the configured email

            if (adminUser == null)
            {
                return;
            }

            // IsInRoleAsync makes this safe to run every time the application starts.
            if (!await userManager.IsInRoleAsync(adminUser, "Admin")) // checks whether the user is not already an Admin
            {
                var addToRoleResult =
                    await userManager.AddToRoleAsync(adminUser, "Admin"); // adds the user to the admin role
                                                                          // Identity creates a row in AspNetUserRoles that links the user's ID and the Admin role's ID
                if (!addToRoleResult.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        addToRoleResult.Errors.Select(error => error.Description));

                    throw new InvalidOperationException(
                        $"Could not assign the Admin role: {errors}");
                }
            }
        }
    }
}