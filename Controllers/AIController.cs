using BoltBrain.Services;
using Microsoft.AspNetCore.Mvc;

namespace BoltBrain.Controllers
{
    public class AIController : Controller
    {
        private readonly GeminiApiClient _geminiClient;

        public AIController(GeminiApiClient geminiClient)
        {
            _geminiClient = geminiClient;
        }

        // GET: AI/Ask
        public IActionResult Ask()
        {
            return View();
        }

        // POST: AI/Ask
        [HttpPost]
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
