using BoltBrain.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BoltBrain.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Study()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Flashcards()
        {
            return View();
        }

        [HttpPost]
        public IActionResult GenerateFlashCards(string studyTopic, int questionAmount)
        {
            Console.WriteLine("Printing data");
            Console.WriteLine(studyTopic);
            Console.WriteLine(questionAmount);
            return RedirectToAction("Flashcards");
            // backend pls take this data
        }

        [HttpPost]
        public IActionResult GenerateQuiz(string studyTopic, int questionAmount)
        {
            Console.WriteLine("Printing data");
            Console.WriteLine(studyTopic);
            Console.WriteLine(questionAmount);
            return RedirectToAction("Study");
            // backend pls take this data
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
