using BoltBrain.Areas.Identity;
using BoltBrain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BoltBrain.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        public DbSet<UserInteraction> UserInteractions { get; set; }

        public DbSet<ErrorLog> ErrorLogs { get; set; }
    }
}
