using BoltBrain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
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
        [Authorize]
        public IActionResult Study()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Flashcards()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Quiz()
        {
            return View();
        }

        [HttpPost]
        public IActionResult GenerateFlashCardsOrQuiz(string studyTopic, int questionAmount, string action)
        {
            if (action == "GenerateFlashCards")
            {
                // Call the method to generate flash cards
                return RedirectToAction("FlashCards", new { topic = studyTopic, amount = questionAmount });
            }
            else if (action == "GenerateQuiz")
            {
                // Call the method to generate a quiz
                return RedirectToAction("Quiz", new { topic = studyTopic, amount = questionAmount });
            }

                // Default action or fallback
                return View();
            }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
