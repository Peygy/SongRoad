namespace MainApp.Models
{
    public class UserRights
    {
        public int Id { get; set; }
        public int WarnCount { get; set; }
        public bool Ban { get; set; } = false;

        public string? UserId { get; set; }
        public UserModel? User { get; set; }
    }
}
