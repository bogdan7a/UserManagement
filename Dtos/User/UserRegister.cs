using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace UserManagement.Dtos.User
{
    public class UserRegister
    {
        [Required(ErrorMessage = "Can't be empty")]
        public string Name { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "Can't be empty")]
        public string Email { get; set; }

        [PasswordPropertyText]
        [Required(ErrorMessage = "Can't be empty")]
        public string Password { get; set; }

        [PasswordPropertyText]
        [Required(ErrorMessage = "Can't be empty")]
        public string PasswordConfirmation { get; set; }
    }
}
