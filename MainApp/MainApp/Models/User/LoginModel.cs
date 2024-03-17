using System.ComponentModel.DataAnnotations;

namespace MainApp.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Не указан логин!")]
        public string UserName { get; set; } = null!;
        [Required(ErrorMessage = "Не указан пароль!")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
    }
}
