using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata.Ecma335;

namespace Thu6.model
{
    public class Users
    {
        
        public string id { get; set; } = string.Empty;

        public string username { get; set; }= string.Empty;
        [Required, MinLength(8), PasswordPropertyText]
        public string password { get; set; } = string.Empty;
        [Required, EmailAddress]
        public string email { get; set; } = string.Empty;
        public string active {  get; set; } = string.Empty;
        public string link_avata  { get; set; }
        public string session { get; set; } = string.Empty;
        public string list_ban {  get; set; }=string.Empty;
        public string coins {  get; set; } = string.Empty;



/*
        public DateTime Created_at { get; set; }

        public DateTime Modified_at { get; set;}*/
    }
}
