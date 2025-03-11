namespace SEB.Models
{
    public class Tournament
    {
        public DateTime StartTime { get; set; } // When the tournament started
        public DateTime EndTime { get; set; } // When the tournament ended
        public List<User> Participants { get; set; } = new List<User>(); // Users who participated
        public Dictionary<User, int> Results { get; set; } = new Dictionary<User, int>(); // Push-up counts per user
        public Dictionary<User, int> EloChanges { get; set; } = new Dictionary<User, int>(); // ELO changes after the tournament
    }
}