using System.ComponentModel.DataAnnotations.Schema;

namespace MainApp.Models
{
    [Table("RefreshTokens")]
    public class RefreshTokenModel
    {
        public string? Id { get; set; }
        [Column(TypeName = "hstore")]
        public Dictionary<string, string> TokensWhiteList { get; set; } = new Dictionary<string, string>();

        public string? UserId { get; set; }
        public UserModel? User { get; set; }
    }
}
