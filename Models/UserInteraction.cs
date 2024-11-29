using BoltBrain.Areas.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BoltBrain.Models
{
    public class UserInteraction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; }

        [Required]
        public string Question { get; set; }

        [Required]
        public string Response { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
