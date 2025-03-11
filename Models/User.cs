namespace SEB.Models
{
    public class User
    {
        public string Username { get; set; } = String.Empty;
        public string Password {get; set; } = String.Empty;
        public int Elo { get; set; } = 0;

        public override string ToString()
        {
            return $"User: {Username}, Elo: {Elo}";
        }
    }
}