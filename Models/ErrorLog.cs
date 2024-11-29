using System.ComponentModel.DataAnnotations;

namespace BoltBrain.Models
{
    public class ErrorLog
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }

        public string Message { get; set; }

        public string StackTrace { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
