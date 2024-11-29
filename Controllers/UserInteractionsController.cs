using BoltBrain.Areas.Identity;
using BoltBrain.Data;
using BoltBrain.Models;
using BoltBrain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BoltBrain.Controllers
{
    [Authorize]
    public class UserInteractionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly GeminiApiClient _geminiClient;

        public UserInteractionsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, GeminiApiClient geminiClient)
        {
            _context = context;
            _userManager = userManager;
            _geminiClient = geminiClient;
        }
        

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var interactions = await _context.UserInteractions
                .Where(ui => ui.UserId == userId)
                .OrderByDescending(ui => ui.Timestamp)
                .ToListAsync();

            return View(interactions);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var interaction = await _context.UserInteractions
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (interaction == null) return NotFound();

            return View(interaction);
        }

      
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Question")] UserInteraction interaction)
        {
            if (ModelState.IsValid)
            {
                interaction.UserId = _userManager.GetUserId(User);
                interaction.Response = await GenerateResponseAsync(interaction.Question); // Implement this method
                interaction.Timestamp = DateTime.UtcNow;

                _context.Add(interaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(interaction);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var interaction = await _context.UserInteractions.FindAsync(id);

            if (interaction == null || interaction.UserId != userId) return NotFound();

            return View(interaction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Question,Response")] UserInteraction interaction)
        {
            if (id != interaction.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var userId = _userManager.GetUserId(User);
                    if (interaction.UserId != userId) return Unauthorized();

                    _context.Update(interaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserInteractionExists(interaction.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(interaction);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var interaction = await _context.UserInteractions
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (interaction == null) return NotFound();

            return View(interaction);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var interaction = await _context.UserInteractions.FindAsync(id);
            if (interaction == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (interaction.UserId != userId) return Unauthorized();

            _context.UserInteractions.Remove(interaction);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserInteractionExists(int id)
        {
            return _context.UserInteractions.Any(e => e.Id == id);
        }

        private async Task<string> GenerateResponseAsync(string question)
        {
            return await _geminiClient.GenerateContentAsync(question);
        }
    }
}
