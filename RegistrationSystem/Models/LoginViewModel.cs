using System.ComponentModel.DataAnnotations;

namespace RegistrationSystem.Models
{
    // Contains the information submitted from
    // the application's custom Login page.
    public class LoginViewModel
    {
        [Required(ErrorMessage = "EmailRequired")]
        [EmailAddress(ErrorMessage = "EmailInvalid")]
        public string Email { get; set; } =
            string.Empty;

        [Required(ErrorMessage = "PasswordRequired")]
        [DataType(DataType.Password)]
        public string Password { get; set; } =
            string.Empty;

        public bool RememberMe { get; set; }

        // Stores the page the user originally requested
        // so they can return there after logging in.
        public string? ReturnUrl { get; set; }
    }
}