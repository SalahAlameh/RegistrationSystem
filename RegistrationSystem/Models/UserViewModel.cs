using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace RegistrationSystem.Models
{
    public class UserListViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public string LinkedPersonName { get; set; } = "Not linked";

        public bool IsDisabled { get; set; }

        public bool IsCurrentUser { get; set; }
    }

    public class CreateUserViewModel
    {
        [Required] // data annotations
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(
            100,
            MinimumLength = 6,
            ErrorMessage = "The password must contain at least 6 characters.")]
        [DataType(DataType.Password)] // masks the characters as the user types
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(
            nameof(Password),
            ErrorMessage = "The password and confirmation password do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Role")]
        public string SelectedRole { get; set; } = "User";

        // PersonId is optional for Admin accounts but required by the
        // service when the selected role is User.
        [Display(Name = "Linked Person")]
        public int? PersonId { get; set; }

        public List<SelectListItem> Roles { get; set; } = new();

        public List<SelectListItem> Persons { get; set; } = new();
    }

    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Role")]
        public string SelectedRole { get; set; } = "User";

        // PersonId is optional for Admin accounts but required by the
        // service when the selected role is User.
        [Display(Name = "Linked Person")]
        public int? PersonId { get; set; }

        public List<SelectListItem> Roles { get; set; } = new();

        public List<SelectListItem> Persons { get; set; } = new();
    }

    public class ResetUserPasswordViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(
            100,
            MinimumLength = 6,
            ErrorMessage = "The password must contain at least 6 characters.")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(
            nameof(NewPassword),
            ErrorMessage = "The password and confirmation password do not match.")]
        [Display(Name = "Confirm New Password")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

}

