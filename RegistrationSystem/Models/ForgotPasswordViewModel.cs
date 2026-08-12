using System.ComponentModel.DataAnnotations;

namespace RegistrationSystem.Models
{
    // Contains the email submitted from the Forgot Password page.
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "EmailRequired")]
        [EmailAddress(ErrorMessage = "EmailInvalid")]
        [Display(Name = "EmailLabel")]
        public string Email { get; set; } = string.Empty;
    }
}
