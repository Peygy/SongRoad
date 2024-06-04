using System.ComponentModel.DataAnnotations;

namespace MainApp.DTO.User
{
    /// <summary>
    /// DTO register model
    /// </summary>
    public class RegisterModelDTO
    {
        [Required(ErrorMessage = "Не указан логин!")]
        [StringLength(20, MinimumLength = 4, ErrorMessage = "Длина логина должна быть от 4 до 20 символов")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "Не указан пароль!")]
        [StringLength(20, MinimumLength = 7, ErrorMessage = "Длина пароля должна быть от 7 до 20 символов")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Не подтвержден пароль!")]
        [Compare("Password", ErrorMessage = "Пароли не совпадают!")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = null!;
    }
}
