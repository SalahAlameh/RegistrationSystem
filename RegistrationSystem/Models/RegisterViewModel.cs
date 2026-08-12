using System.ComponentModel.DataAnnotations;

namespace RegistrationSystem.Models
{
    // Contains the information submitted from the public registration page.
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "EmailRequired")]
        [EmailAddress(ErrorMessage = "EmailInvalid")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "PasswordRequired")]
        [StringLength(
            100,
            MinimumLength = 6,
            ErrorMessage = "PasswordMinimumLength")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "ConfirmPasswordRequired")]
        [DataType(DataType.Password)]
        [Compare(
            nameof(Password),
            ErrorMessage = "PasswordsDoNotMatch")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }
}