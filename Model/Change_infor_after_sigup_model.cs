using System.ComponentModel.DataAnnotations;

namespace thu3.Model
{
    public class Change_infor_after_sigup_model
    {
        [Required]
        public string token { get; set; }=string.Empty;
        [Required]
        public string username {  get; set; }=string.Empty;
        public IFormFile avatar { get; set; }
    }
}
