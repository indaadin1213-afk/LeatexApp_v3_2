
namespace LeatexApp.Models
{
    public enum Uloga { Radnik, Direktor }

    public class User
    {
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Salt { get; set; } = string.Empty;
        public Uloga Role { get; set; } = Uloga.Radnik;
    }
}
