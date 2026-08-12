using System.ComponentModel.DataAnnotations;

namespace RegistrationSystem.Models
{
    // Contains the recovery token and new password submitted
    // from the public Reset Password page.
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "EmailRequired")]
        [EmailAddress(ErrorMessage = "EmailInvalid")]
        [Display(Name = "EmailLabel")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "NewPasswordRequired")]
        [StringLength(
            100,
            MinimumLength = 6,
            ErrorMessage = "PasswordMinimumLength")]
        [DataType(DataType.Password)]
        [Display(Name = "NewPasswordLabel")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "ConfirmNewPasswordRequired")]
        [DataType(DataType.Password)]
        [Compare(
            nameof(Password),
            ErrorMessage = "PasswordsDoNotMatch")]
        [Display(Name = "ConfirmNewPasswordLabel")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "PasswordResetCodeRequired")]
        public string Code { get; set; } = string.Empty;
    }
}
