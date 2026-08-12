using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace RegistrationSystem.Models
{
    public class UserListViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public string? LinkedPersonName { get; set; }

        public bool IsDisabled { get; set; }

        public bool IsCurrentUser { get; set; }
    }

    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "EmailRequired")] // data annotations
        [EmailAddress(ErrorMessage = "EmailInvalid")]
        [Display(Name = "EmailLabel")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "PasswordRequired")]
        [StringLength(
            100,
            MinimumLength = 6,
            ErrorMessage = "PasswordMinimumLength")]
        [DataType(DataType.Password)] // masks the characters as the user types
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "ConfirmPasswordRequired")]
        [DataType(DataType.Password)]
        [Compare(
            nameof(Password),
            ErrorMessage = "PasswordsDoNotMatch")]
        [Display(Name = "ConfirmPasswordLabel")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "UserRoleRequired")]
        [Display(Name = "UserRoleLabel")]
        public string SelectedRole { get; set; } = "User";

        // PersonId is optional for Admin accounts but required by the
        // service when the selected role is User.
        [Display(Name = "LinkedPersonLabel")]
        public int? PersonId { get; set; }

        public List<SelectListItem> Roles { get; set; } = new();

        public List<SelectListItem> Persons { get; set; } = new();
    }

    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "EmailRequired")]
        [EmailAddress(ErrorMessage = "EmailInvalid")]
        [Display(Name = "EmailLabel")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "UserRoleRequired")]
        [Display(Name = "UserRoleLabel")]
        public string SelectedRole { get; set; } = "User";

        // PersonId is optional for Admin accounts but required by the
        // service when the selected role is User.
        [Display(Name = "LinkedPersonLabel")]
        public int? PersonId { get; set; }

        public List<SelectListItem> Roles { get; set; } = new();

        public List<SelectListItem> Persons { get; set; } = new();
    }

    public class ResetUserPasswordViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Display(Name = "EmailLabel")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "NewPasswordRequired")]
        [StringLength(
            100,
            MinimumLength = 6,
            ErrorMessage = "PasswordMinimumLength")]
        [DataType(DataType.Password)]
        [Display(Name = "NewPasswordLabel")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "ConfirmNewPasswordRequired")]
        [DataType(DataType.Password)]
        [Compare(
            nameof(NewPassword),
            ErrorMessage = "PasswordsDoNotMatch")]
        [Display(Name = "ConfirmNewPasswordLabel")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

}

