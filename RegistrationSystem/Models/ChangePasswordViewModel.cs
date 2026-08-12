using System.ComponentModel.DataAnnotations;

namespace RegistrationSystem.Models
{
    // Contains the current and new passwords submitted
    // by a logged-in administrator.
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "CurrentPasswordRequired")]
        [DataType(DataType.Password)]
        [Display(Name = "CurrentPasswordLabel")]
        public string CurrentPassword { get; set; } = string.Empty;

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
