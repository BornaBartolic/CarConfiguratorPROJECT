using System.ComponentModel.DataAnnotations;

namespace CarConfigPROJECTmvc.ViewModels.User
{
    public class UserLoginVM
    {
        [Required(ErrorMessage = "User name is required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
        
        public string ReturnUrl { get; set; }
    }
}
