namespace SEB.Models
{
    public class PushUpRecord
    {
        public int Count { get; set; } // Number of push-ups
        public TimeSpan Duration { get; set; } // Duration of the exercise (e.g., 2 minutes)
        public DateTime Timestamp { get; set; } = DateTime.UtcNow; // When the push-ups were recorded
    }
}