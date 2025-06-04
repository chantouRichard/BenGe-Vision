namespace picture_backend.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;

        public ICollection<Script> Scripts { get; set; } = new List<Script>();
    }
}
