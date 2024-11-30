using BoltBrain.Areas.Identity;
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
        public async Task<IActionResult> Ask(string studyTopic, int questionAmount, string action)
        {
            if (string.IsNullOrWhiteSpace(studyTopic) || questionAmount <= 0)
            {
                ModelState.AddModelError("", "Please provide a valid study topic and number of questions.");
                return RedirectToAction("Study", "Home");
            }

            try
            {
                string prompt = action switch
                {
                    "GenerateFlashCards" => $"Please generate {questionAmount} flashcards on the topic: {studyTopic}. Each flashcard should have a question and an answer.",
                    "GenerateQuiz" => $"Please create a quiz with {questionAmount} multiple-choice questions on the topic: {studyTopic}. Each question should have one correct answer and three incorrect options.",
                    _ => null
                };

                if (string.IsNullOrEmpty(prompt))
                {
                    ModelState.AddModelError("", "Invalid action selected.");
                    return RedirectToAction("Study", "Home");
                }

                var response = await _geminiClient.GenerateContentAsync(prompt);

                var interaction = new UserInteraction
                {
                    UserId = _userManager.GetUserId(User),
                    Question = prompt,
                    Response = response,
                    Timestamp = DateTime.UtcNow
                };

                _context.UserInteractions.Add(interaction);
                await _context.SaveChangesAsync();

                TempData["Response"] = response;
                TempData["StudyTopic"] = studyTopic;
                TempData["QuestionAmount"] = questionAmount;

                if (action == "GenerateFlashCards")
                {
                    return RedirectToAction("Flashcards", "AI");
                }
                else if (action == "GenerateQuiz")
                {
                    return RedirectToAction("Quiz", "AI");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid action.");
                    return RedirectToAction("Study", "Home");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                return RedirectToAction("Study", "Home");
            }
        }

        [HttpGet]
        public IActionResult Flashcards()
        {
            var response = TempData["Response"] as string;
            var studyTopic = TempData["StudyTopic"] as string;
            var questionAmount = TempData["QuestionAmount"];

            if (string.IsNullOrEmpty(response))
            {
                return RedirectToAction("Study", "Home");
            }

            ViewBag.StudyTopic = studyTopic;
            ViewBag.QuestionAmount = questionAmount;

            return View(model: response);
        }

        [HttpGet]
        public IActionResult Quiz()
        {
            var response = TempData["Response"] as string;
            var studyTopic = TempData["StudyTopic"] as string;
            var questionAmount = TempData["QuestionAmount"];

            if (string.IsNullOrEmpty(response))
            {
                return RedirectToAction("Study", "Home");
            }

            ViewBag.StudyTopic = studyTopic;
            ViewBag.QuestionAmount = questionAmount;

            return View(model: response);
        }
    }
}
