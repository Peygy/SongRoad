using System.ComponentModel.DataAnnotations.Schema;

namespace MainApp.Models
{
    /// <summary>
    /// Model of refresh token
    /// </summary>
    [Table("RefreshTokens")]
    public class RefreshTokenModel
    {
        public string? Id { get; set; }
        // IP: refreshToken
        [Column(TypeName = "hstore")]
        public Dictionary<string, string> TokensWhiteList { get; set; } = new Dictionary<string, string>();

        public string? UserId { get; set; }
        public UserModel? User { get; set; }
    }
}
