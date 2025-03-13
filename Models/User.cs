namespace SEB.Models
{
    public class User
    {
        public int Id { get; set; } 
        public string Username { get; set; } = string.Empty;
        public string Password {get; set; } = string.Empty;
        public int Elo { get; set; } = 0;
        public List<PushUpRecord> PushUpHistory { get; set; } = new List<PushUpRecord>();

        public override string ToString()
        {
            return $"User: {Username}, Elo: {Elo}";
        }
    }
}