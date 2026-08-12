using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RegistrationSystem.Data;
using RegistrationSystem.Models;

namespace RegistrationSystem.Services
{
    public class UserOperationResult // this class is used to return the result of a user operation, such as creating or updating a user. It contains information about whether the operation succeeded, whether the user was not found, and any error messages that occurred during the operation.
    {
        public bool Succeeded { get; init; }

        public bool NotFound { get; init; }

        // Stores resource keys so the controller can translate errors
        // using the language selected for the current request.
        public List<string> Errors { get; init; } = new();

        public static UserOperationResult Success()
        {
            return new UserOperationResult
            {
                Succeeded = true
            };
        }

        public static UserOperationResult NotFoundResult()
        {
            return new UserOperationResult
            {
                NotFound = true
            };
        }

        public static UserOperationResult Failed(
            params string[] errors)
        {
            return new UserOperationResult
            {
                Errors = errors.ToList()
            };
        }
    }

    public class UserService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public UserService(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }


        // loads all Identity users and the roles assigned to each user.
        public async Task<List<UserListViewModel>> GetAllAsync( // this method prepares the Users list page
            ClaimsPrincipal currentUser)
        {
            var currentUserId =
                _userManager.GetUserId(currentUser);

            var identityUsers = await _userManager.Users
                .OrderBy(user => user.Email)
                .ToListAsync();

            var linkedPeople = await _context.Persons // loads linked people
                .Where(person => person.IdentityUserId != null)
                .ToDictionaryAsync(
                    person => person.IdentityUserId!,
                    person => person.FullName);

            var model = new List<UserListViewModel>();

            foreach (var identityUser in identityUsers)
            {
                var roles =
                    await _userManager.GetRolesAsync(identityUser);

                linkedPeople.TryGetValue(
                    identityUser.Id,
                    out var linkedPersonName);

                model.Add(new UserListViewModel
                { // the ?? means "use the value on the right if the value on the left is null"
                    Id = identityUser.Id,
                    Email = identityUser.Email
                        ?? identityUser.UserName
                        ?? string.Empty,
                    Role = roles.Any()
                        ? string.Join(", ", roles)
                        : string.Empty,
                    LinkedPersonName =
                        linkedPersonName, // each user view model receives the linked person's name
                    IsDisabled = // determines whether the account is currently deactivated
                        identityUser.LockoutEnd.HasValue && // checks whether LockoutEnd has a date
                        identityUser.LockoutEnd.Value > DateTimeOffset.UtcNow, // checks whether that date is in the future
                    IsCurrentUser =
                        identityUser.Id == currentUserId // this compares the account being displayed with the currently logged in administrator, if yes, this shows the you badge and disables the deactivate button for that row
                });
            }

            return model;
        }

        // Builds the complete model needed by the Create User form.
        public async Task<CreateUserViewModel> GetCreateModelAsync() // Complete Create User form
        {
            var model = new CreateUserViewModel // creates a new CreateUserViewModel object with the default role set to "User"
            {
                SelectedRole = "User"
            };

            await PopulateCreateOptionsAsync(model); // this method populates the Roles and Persons dropdowns in the Create form

            return model;
        }

        // Reloads dropdown data when the Create form must be redisplayed.
        public async Task PopulateCreateOptionsAsync( // Redisplayed Create form after an error
            CreateUserViewModel model)
        {
            model.Roles =
                await GetRoleItemsAsync(model.SelectedRole);

            model.Persons =
                await GetPersonItemsAsync(
                    model.PersonId,
                    null);
        }

        // Reloads dropdown data when the Edit form must be redisplayed.
        public async Task PopulateEditOptionsAsync( // Redisplayed Edit form after an error
            EditUserViewModel model)
        {
            model.Roles =
                await GetRoleItemsAsync(model.SelectedRole);

            model.Persons =
                await GetPersonItemsAsync(
                    model.PersonId,
                    model.Id);
        }

        // loads the available roles for a form dropdown.
        private async Task<List<SelectListItem>> GetRoleItemsAsync( // Role dropdown
            string? selectedRole) // this loads roles for the Create and Edit dropdowns
        {
            return await _roleManager.Roles
                .OrderBy(role => role.Name)
                .Select(role => new SelectListItem
                {
                    Value = role.Name,
                    Text = role.Name,
                    Selected = role.Name == selectedRole
                })
                .ToListAsync();
        }

        // Loads active people who are not linked to another account.
        private async Task<List<SelectListItem>> GetPersonItemsAsync( // Linked Person dropdown
            int? selectedPersonId,
            string? currentIdentityUserId)
        {
            return await _context.Persons
                .Where(person =>
                    !person.IsDeleted &&
                    (person.IdentityUserId == null ||
                     person.IdentityUserId == currentIdentityUserId))
                .OrderBy(person => person.FullName)
                .Select(person => new SelectListItem
                {
                    Value = person.Id.ToString(),
                    Text = person.FullName,
                    Selected = person.Id == selectedPersonId
                })
                .ToListAsync();
        }

        // loads one user for the edit form.
        public async Task<EditUserViewModel?> GetEditModelAsync( // Complete Edit User form
            string id) // this retrieves one identity account for the Edit page
        {
            var identityUser =
                await _userManager.FindByIdAsync(id);

            if (identityUser == null)
            {
                return null;
            }

            var roles =
                await _userManager.GetRolesAsync(identityUser);

            var selectedRole =
                roles.FirstOrDefault() ?? "User";

            var linkedPerson = await _context.Persons
                .FirstOrDefaultAsync(person =>
                    person.IdentityUserId == identityUser.Id);

            var model = new EditUserViewModel // if the account is found, the service builds EditUserViewModel
            {
                Id = identityUser.Id,
                Email = identityUser.Email
                    ?? identityUser.UserName
                    ?? string.Empty,
                SelectedRole = selectedRole,
                PersonId = linkedPerson?.Id // places that Person ID int othe edit model, that is why the existing linked Person appears selected automatically in the dropdown
            };

            await PopulateEditOptionsAsync(model);

            return model;
        }

        // creates a new Identity account and assigns its selected role.
        public async Task<UserOperationResult> CreateAsync(
            CreateUserViewModel model)
        {
            if (!await _roleManager.RoleExistsAsync( // first it verifies that the selected role exists
                    model.SelectedRole))
            {
                return UserOperationResult.Failed(
                    "UserSelectedRoleNotFound");
            }

            if (model.SelectedRole == "User" && // regular users must be linked to a person
                !model.PersonId.HasValue)
            {
                return UserOperationResult.Failed(
                    "UserAccountRequiresLinkedPerson");
            }

            Person? selectedPerson = null;

            if (model.PersonId.HasValue)
            {
                selectedPerson = await _context.Persons // makes sure the selected Person exists, is not archived, and is not already linked to another account
                    .FirstOrDefaultAsync(person =>
                        person.Id == model.PersonId.Value &&
                        !person.IsDeleted);

                if (selectedPerson == null)
                {
                    return UserOperationResult.Failed(
                        "UserSelectedPersonUnavailable");
                }

                if (selectedPerson.IdentityUserId != null) // this protects the one account per person rule
                {
                    return UserOperationResult.Failed(
                        "UserPersonAlreadyLinked");
                }
            }

            var email = model.Email.Trim();

            var identityUser = new IdentityUser // then it creates a new IdentityUser object with the provided email and password
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                LockoutEnabled = true
            };

            var createResult =
                await _userManager.CreateAsync(
                    identityUser,
                    model.Password); // the password is passed to Identity

            if (!createResult.Succeeded)
            {
                return FromIdentityResult(createResult);
            }

            // Identity then validates the password, hashes it, and stores the account in AspNetUsers table
            var roleResult =
                await _userManager.AddToRoleAsync( // after the account is created, the role is assigned
                    identityUser,
                    model.SelectedRole);

            if (!roleResult.Succeeded)
            {
                // if role assignment fails, remove the newly created
                // account so an incomplete user is not left behind.
                await _userManager.DeleteAsync(identityUser);

                return FromIdentityResult(roleResult);
            }

            if (selectedPerson != null)
            {
                selectedPerson.IdentityUserId = identityUser.Id; // this links the newly created account to the selected Person by setting the IdentityUserId property of the Person entity to the Id of the newly created IdentityUser

                try
                {
                    // All current application services use synchronous
                    // SaveChanges so the existing audit override is applied.
                    _context.SaveChanges();
                }
                catch (DbUpdateException)
                {
                    selectedPerson.IdentityUserId = null;
                    _context.Entry(selectedPerson).State =
                        EntityState.Unchanged;

                    await _userManager.DeleteAsync(identityUser);

                    return UserOperationResult.Failed(
                        "UserLinkFailed");
                }
            }

            return UserOperationResult.Success();
        }

        // updates a user's email address, role, and linked person.
        public async Task<UserOperationResult> UpdateAsync(
            EditUserViewModel model,
            ClaimsPrincipal currentUser)
        {
            var identityUser =
                await _userManager.FindByIdAsync(model.Id);

            if (identityUser == null)
            {
                return UserOperationResult.NotFoundResult();
            }

            if (!await _roleManager.RoleExistsAsync(
                    model.SelectedRole))
            {
                return UserOperationResult.Failed(
                    "UserSelectedRoleNotFound");
            }

            var currentUserId =
                _userManager.GetUserId(currentUser);

            // Prevent an administrator from removing their own
            // Admin role and locking themselves out.
            if (identityUser.Id == currentUserId &&
                model.SelectedRole != "Admin")
            {
                return UserOperationResult.Failed(
                    "UserCannotRemoveOwnAdminRole");
            }

            if (model.SelectedRole == "User" &&
                !model.PersonId.HasValue)
            {
                return UserOperationResult.Failed(
                    "UserAccountRequiresLinkedPerson");
            }

            var currentlyLinkedPerson = await _context.Persons
                .FirstOrDefaultAsync(person =>
                    person.IdentityUserId == identityUser.Id);

            Person? selectedPerson = null;

            if (model.PersonId.HasValue)
            {
                selectedPerson = await _context.Persons
                    .FirstOrDefaultAsync(person =>
                        person.Id == model.PersonId.Value &&
                        !person.IsDeleted);

                if (selectedPerson == null)
                {
                    return UserOperationResult.Failed(
                        "UserSelectedPersonUnavailable");
                }

                if (selectedPerson.IdentityUserId != null &&
                    selectedPerson.IdentityUserId != identityUser.Id)
                {
                    return UserOperationResult.Failed(
                        "UserPersonAlreadyLinked");
                }
            }

            var newEmail = model.Email.Trim(); // trims whitespace from the email address

            if (!string.Equals( // this ckecks whether the new user's email has acutally changed before updating it
                    identityUser.Email,
                    newEmail,
                    StringComparison.OrdinalIgnoreCase))
            {
                var emailResult =
                    await _userManager.SetEmailAsync( // this updates the email address in the AspNetUsers table
                        identityUser,
                        newEmail);

                if (!emailResult.Succeeded)
                {
                    return FromIdentityResult(emailResult); // if the email update fails, the method returns an error result with the details of the failure
                }
            }

            // This project uses the email address as the username,
            // so both values must be changed together.
            if (!string.Equals(
                    identityUser.UserName,
                    newEmail,
                    StringComparison.OrdinalIgnoreCase))
            {
                var usernameResult =
                    await _userManager.SetUserNameAsync(
                        identityUser,
                        newEmail);

                if (!usernameResult.Succeeded)
                {
                    return FromIdentityResult(usernameResult);
                }
            }

            var currentRoles =
                await _userManager.GetRolesAsync(identityUser);

            // Add the new role first so the user is never
            // temporarily left without a role.
            if (!currentRoles.Contains(model.SelectedRole))
            {
                var addRoleResult =
                    await _userManager.AddToRoleAsync(
                        identityUser,
                        model.SelectedRole);

                if (!addRoleResult.Succeeded)
                {
                    return FromIdentityResult(addRoleResult);
                }
            }

            var rolesToRemove = currentRoles
                .Where(role => role != model.SelectedRole)
                .ToList();

            if (rolesToRemove.Any())
            {
                var removeRolesResult =
                    await _userManager.RemoveFromRolesAsync(
                        identityUser,
                        rolesToRemove);

                if (!removeRolesResult.Succeeded)
                {
                    return FromIdentityResult(removeRolesResult);
                }
            }

            if (currentlyLinkedPerson?.Id != selectedPerson?.Id)
            {
                if (currentlyLinkedPerson != null) // when an Admin changes the linked Person, the service first clears the old link
                {
                    currentlyLinkedPerson.IdentityUserId = null;
                }

                if (selectedPerson != null) // then it assigns the new link
                {
                    selectedPerson.IdentityUserId = identityUser.Id;
                }

                try
                {
                    _context.SaveChanges();
                }
                catch (DbUpdateException)
                {
                    return UserOperationResult.Failed(
                        "UserLinkFailed");
                }
            }

            return UserOperationResult.Success();
        }

        // Builds the form used by an administrator to reset another user's password.
        public async Task<ResetUserPasswordViewModel?>
            GetResetPasswordModelAsync(string id)
        {
            var identityUser =
                await _userManager.FindByIdAsync(id);

            if (identityUser == null)
            {
                return null;
            }

            return new ResetUserPasswordViewModel
            {
                Id = identityUser.Id,
                Email = identityUser.Email
                    ?? identityUser.UserName
                    ?? string.Empty
            };
        }

        // Resets another user's password without requiring their old password.
        public async Task<UserOperationResult> ResetPasswordAsync(
            ResetUserPasswordViewModel model,
            ClaimsPrincipal currentUser)
        {
            var identityUser =
                await _userManager.FindByIdAsync(model.Id);

            if (identityUser == null)
            {
                return UserOperationResult.NotFoundResult();
            }

            // Administrators should use Change Password for their own account,
            // because that process confirms their current password first.
            if (identityUser.Id ==
                _userManager.GetUserId(currentUser))
            {
                return UserOperationResult.Failed(
                    "UserUseChangePasswordForOwnAccount");
            }

            // Identity requires a secure, temporary reset token before it
            // allows a password to be reset without the old password.
            var resetToken =
                await _userManager.GeneratePasswordResetTokenAsync(
                    identityUser);

            var resetResult =
                await _userManager.ResetPasswordAsync(
                    identityUser,
                    resetToken,
                    model.NewPassword);

            if (!resetResult.Succeeded)
            {
                return FromIdentityResult(resetResult);
            }

            return UserOperationResult.Success();
        }

        // Deactivates an account without permanently deleting it.
        public async Task<UserOperationResult> DisableAsync(
            string id,
            ClaimsPrincipal currentUser)
        {
            var identityUser =
                await _userManager.FindByIdAsync(id);

            if (identityUser == null)
            {
                return UserOperationResult.NotFoundResult();
            }

            // Administrators cannot deactivate the account
            // they are currently using.
            if (identityUser.Id ==
                _userManager.GetUserId(currentUser))
            {
                return UserOperationResult.Failed(
                    "UserCannotDeactivateOwnAccount");
            }

            identityUser.LockoutEnabled = true;
            identityUser.LockoutEnd = DateTimeOffset.MaxValue; // this sets the LockoutEnd property to a date far in the future, effectively deactivating the account

            var result =
                await _userManager.UpdateAsync(identityUser);

            if (!result.Succeeded)
            {
                return FromIdentityResult(result);
            }

            // Changes the security stamp so an existing login
            // session will be invalidated during Identity validation.
            await _userManager.UpdateSecurityStampAsync(
                identityUser); // basically signs the user out of any active sessions, forcing them to log in again if they try to access the system while their account is deactivated

            return UserOperationResult.Success();
        }

        // Reactivates a previously deactivated account.
        public async Task<UserOperationResult> EnableAsync(
            string id)
        {
            var identityUser =
                await _userManager.FindByIdAsync(id);

            if (identityUser == null)
            {
                return UserOperationResult.NotFoundResult();
            }

            identityUser.LockoutEnabled = true;
            identityUser.LockoutEnd = null;

            var result =
                await _userManager.UpdateAsync(identityUser);

            if (!result.Succeeded)
            {
                return FromIdentityResult(result);
            }

            return UserOperationResult.Success();
        }

        // helper method that converts those errors into our application-level result
        private static UserOperationResult FromIdentityResult(
            IdentityResult result)
        {
            return new UserOperationResult
            {
                Errors = result.Errors
                    .Select(error => error.Code)
                    .ToList()
            };
        }
    }
}
