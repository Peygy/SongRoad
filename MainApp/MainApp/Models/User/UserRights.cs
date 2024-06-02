namespace MainApp.Models
{
    /// <summary>
    /// Model of user rights for content access (denied - banned; warns max 3)
    /// </summary>
    public class UserRights
    {
        public int Id { get; set; }
        public int WarnCount { get; set; }
        public bool Banned { get; set; } = false;

        public string? UserId { get; set; }
        public UserModel? User { get; set; }
    }
}
