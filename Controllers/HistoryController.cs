using BoltBrain.Areas.Identity;
using BoltBrain.Data;
using BoltBrain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BoltBrain.Controllers
{
    [Authorize]
    public class HistoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HistoryController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: History/Index
        public async Task<IActionResult> Index(string searchTerm, int? page)
        {
            var userId = _userManager.GetUserId(User);
            int pageSize = 10; 
            int pageNumber = page ?? 1;

            var interactionsQuery = _context.UserInteractions
                .Where(ui => ui.UserId == userId);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                interactionsQuery = interactionsQuery.Where(ui => ui.Question.Contains(searchTerm));
            }

            interactionsQuery = interactionsQuery.OrderByDescending(ui => ui.Timestamp);

            var pagedInteractions = await interactionsQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling(interactionsQuery.Count() / (double)pageSize);

            return View(pagedInteractions);
        }

        // GET: History/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);

            var interaction = await _context.UserInteractions
                .FirstOrDefaultAsync(ui => ui.Id == id && ui.UserId == userId);

            if (interaction == null)
            {
                return NotFound();
            }

            return View(interaction);
        }
    }
}
