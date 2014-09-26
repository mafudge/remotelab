using RemoteLab.CustomValidators;
using System.ComponentModel.DataAnnotations;

namespace RemoteLab.Models
{

    public class LoginViewModel
    {
        [Required]
        [Display(Name = "User name")]
        [NotAnEmail]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

    }

}
