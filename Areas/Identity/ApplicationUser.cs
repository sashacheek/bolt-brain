using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BoltBrain.Areas.Identity
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(100)]
        public string FullName { get; set; }
       
    }
}
