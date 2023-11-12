using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;

namespace thu3.Model
{
    public class LoginModel
    {
        [Required, MinLength(8), PasswordPropertyText]
        public string password { get; set; } = string.Empty;
        [Required, EmailAddress]
        public string email { get; set; } = string.Empty;
        [Required(ErrorMessage ="DevToken is required"), FromHeader]
        public string devtoken {  get; set; }=string.Empty;
    }
}
