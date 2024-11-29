using BoltBrain.Data;
using BoltBrain.Models;
using BoltBrain.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BoltBrain.Controllers
{
    public class AIController : Controller
    {
        private readonly GeminiApiClient _geminiClient;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AIController(
            GeminiApiClient geminiClient,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _geminiClient = geminiClient;
            _context = context;
            _userManager = userManager;
        }
        
        // GET: AI/Ask
        public IActionResult Ask()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ask(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
            {
                ModelState.AddModelError("", "Your question cannot be empty.");
                return View();
            }

            try
            {
                var response = await _geminiClient.GenerateContentAsync(question);

                var interaction = new UserInteraction
                {
                    UserId = _userManager.GetUserId(User),
                    Question = question,
                    Response = response,
                    Timestamp = DateTime.UtcNow
                };

                _context.UserInteractions.Add(interaction);
                await _context.SaveChangesAsync();

                ViewBag.Response = response;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
            }

            return View();
        }
    }
}
