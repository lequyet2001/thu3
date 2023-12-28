using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata.Ecma335;
using thu3.Model;

namespace Thu6.model
{
    public class Users
    {
        
        public string id { get; set; } = string.Empty;

        public string usename { get; set; }= string.Empty;
        [Required, MinLength(8), PasswordPropertyText]
        public string password { get; set; } = string.Empty;
        [Required, EmailAddress]
        public string email { get; set; } = string.Empty;
        public int active {  get; set; }
        public string link_avata { get; set; } 
        public string session { get; set; } = string.Empty;
        public List<banned> list_ban {  get; set; }
        public int coins {  get; set; } 
        public DateTime created { get; set; }
        public string banned { get; set; }=string.Empty;
        public string description { get; set; }=string.Empty;
        public string address { get; set; }=string.Empty;
        public string country { get; set; }=string.Empty;
        public string city { get; set; }=string.Empty;
        public string link { get; set; }=string.Empty;
        public string cover_image { get; set; }=string.Empty;
/*        public DateTime Created_at { get; set; }

        public DateTime Modified_at { get; set;}*/
    }
}
