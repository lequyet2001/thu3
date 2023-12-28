using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace thu3.Model
{
    public class SignupModel
    {
        [Required(ErrorMessage = " Email is required"), EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*\W).*$",
    ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character")]
        [MaxLength(30, ErrorMessage = "Password must be at most 30   characters long")]
        [PasswordPropertyText]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage ="UuId is required")]
        public string Uuid { get; set; }=string.Empty;
    }
}
