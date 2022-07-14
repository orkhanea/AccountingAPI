using System.ComponentModel.DataAnnotations;

namespace WebSuperApi.ModelStates
{
    public class LoginModel
    {
        [Required(ErrorMessage = "User Name is required")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }   
    }
}
