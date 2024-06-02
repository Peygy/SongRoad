using System.ComponentModel.DataAnnotations;

namespace MainApp.DTO.User
{
    /// <summary>
    /// DTO login model
    /// </summary>
    public class LoginModelDTO
    {
        [Required(ErrorMessage = "Не указан логин!")]
        public string UserName { get; set; } = null!;
        [Required(ErrorMessage = "Не указан пароль!")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
    }
}
