using BoltBrain.Areas.Identity;
using BoltBrain.Data;
using BoltBrain.Models;
using BoltBrain.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Linq;
using System.Security.Policy;

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
                    "GenerateFlashCards" => $"Please generate {questionAmount} flashcards on the topic: {studyTopic}. Each flashcard should have a question and an answer. Please format as &-separated-values in frontofcard1&backofcard1&frontofcard2&backofcard2 format",
                    "GenerateQuiz" => $"Please create a quiz with {questionAmount} multiple-choice questions on the topic: {studyTopic}. Each question should have one correct answer and three incorrect options. Please format as &-separated-values in question1&correctanswer&wronganswer&wronganswer&wronganswer&question2&correctanswer&wronganswer&wronganswer&wronganswer format.",
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
                string[] responses = response.Split("&");
                responses = Array.ConvertAll(responses, s => s.Trim());
                //TempData["Response"] = response;
                //TempData["StudyTopic"] = studyTopic;
                //TempData["QuestionAmount"] = questionAmount;
                HttpContext.Session.SetString("StudyTopic", studyTopic);
                HttpContext.Session.SetInt32("QuestionAmount", questionAmount);


                if (action == "GenerateFlashCards")
                {
                        HttpContext.Session.SetString("flashcards", JsonSerializer.Serialize(responses));
                        HttpContext.Session.SetInt32("currentIndex", 0);

                    var storedFlashcards = JsonSerializer.Deserialize<string[]>(HttpContext.Session.GetString("flashcards"));
                    int currentIndex = HttpContext.Session.GetInt32("currentIndex") ?? 0;

                    string[] flashcard = { storedFlashcards[currentIndex], storedFlashcards[currentIndex + 1] } ;;
                    TempData["Flashcard"] = flashcard;

                    return RedirectToAction("Flashcards", "AI");
                }
                else if (action == "GenerateQuiz")
                {
                    HttpContext.Session.SetString("questions", JsonSerializer.Serialize(responses));
                    HttpContext.Session.SetInt32("currentIndex", 0);

                    string[] answers = new string[responses.Length / 5];
                    HttpContext.Session.SetString("answers", JsonSerializer.Serialize(answers));

                    var storedQuestions = JsonSerializer.Deserialize<string[]>(HttpContext.Session.GetString("questions"));
                    int currentIndex = HttpContext.Session.GetInt32("currentIndex") ?? 0;

                    // shuffle questions as questionSet
                    string[] shuffledQuestions = new string[responses.Length];
                    for (int i = 0; i < storedQuestions.Length; i += 5)
                    {
                        shuffledQuestions[i] = storedQuestions[i];
                        var subarr = ShuffleArray(storedQuestions.Skip(i + 1).Take(4).ToArray());
                        for (int j = 0; j < subarr.Length; j++)
                        {
                            shuffledQuestions[i + j + 1] = subarr[j];
                        }
                    }

                    HttpContext.Session.SetString("shuffledQuestions", JsonSerializer.Serialize(shuffledQuestions));

                    string[] question = { shuffledQuestions[currentIndex], shuffledQuestions[currentIndex + 1], shuffledQuestions[currentIndex + 2], shuffledQuestions[currentIndex + 3], shuffledQuestions[currentIndex + 4] }; ;
                    TempData["Question"] = question;

                    return RedirectToAction("Quiz", "AI");
                }
                else
                {
                    Console.WriteLine("Error");
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
            int currentIndex = HttpContext.Session.GetInt32("currentIndex") ?? 0;
            int realIndex = (currentIndex / 2) + 1;

            var flashcard = TempData["Flashcard"] as string[];

            //var response = TempData["Response"] as string;
            var studyTopic = HttpContext.Session.GetString("StudyTopic");
            var questionAmount = HttpContext.Session.GetInt32("QuestionAmount") ?? 0; ;

            //if (string.IsNullOrEmpty(response))
            //{
            //    return RedirectToAction("Study", "Home");
            //}

            ViewBag.StudyTopic = studyTopic;
            ViewBag.QuestionAmount = questionAmount;
            ViewBag.Index = realIndex;

            return View(model: flashcard);
        }

        [HttpGet]
        public IActionResult NextFlashcard()
        {
            int currentIndex = HttpContext.Session.GetInt32("currentIndex") ?? 0;
            currentIndex += 2;
            HttpContext.Session.SetInt32("currentIndex", currentIndex);

            var storedFlashcards = JsonSerializer.Deserialize<string[]>(HttpContext.Session.GetString("flashcards"));

            string[] flashcard = { storedFlashcards[currentIndex], storedFlashcards[currentIndex + 1] }; ;
            TempData["Flashcard"] = flashcard;

            if ((currentIndex / 2) + 1 > HttpContext.Session.GetInt32("QuestionAmount"))
            {
                return RedirectToAction("Study", "Home");
            }

            return RedirectToAction("Flashcards", "AI");
        }

        [HttpGet]
        public IActionResult PreviousFlashcard()
        {
            int currentIndex = HttpContext.Session.GetInt32("currentIndex") ?? 0;
            currentIndex -= 2;
            HttpContext.Session.SetInt32("currentIndex", currentIndex);

            var storedFlashcards = JsonSerializer.Deserialize<string[]>(HttpContext.Session.GetString("flashcards"));

            string[] flashcard = { storedFlashcards[currentIndex], storedFlashcards[currentIndex + 1] }; ;
            TempData["Flashcard"] = flashcard;

            return RedirectToAction("Flashcards", "AI");
        }

        [HttpGet]
        public IActionResult Quiz()
        {
            int currentIndex = HttpContext.Session.GetInt32("currentIndex") ?? 0;
            int realIndex = (currentIndex / 5) + 1;

            var question = TempData["Question"] as string[];

            var studyTopic = HttpContext.Session.GetString("StudyTopic");
            var questionAmount = HttpContext.Session.GetInt32("QuestionAmount") ?? 0; ;

            var answers = JsonSerializer.Deserialize<string[]>(HttpContext.Session.GetString("answers"));
            ViewBag.Answer = answers[realIndex - 1];

            ViewBag.StudyTopic = studyTopic;
            ViewBag.QuestionAmount = questionAmount;
            ViewBag.Index = realIndex;

            return View(model: question);
        }

        [HttpPost]
        public IActionResult QuizNewQuestion(string answer, string action)
        {
            int currentIndex = HttpContext.Session.GetInt32("currentIndex") ?? 0;
            int realIndex = (currentIndex / 5) + 1;

            if (!string.IsNullOrEmpty(answer))
            {
                string[] answers = JsonSerializer.Deserialize<string[]>(HttpContext.Session.GetString("answers"));
                answers[realIndex - 1] = answer;
                HttpContext.Session.SetString("answers", JsonSerializer.Serialize(answers));
            }

            if (action == "PreviousQuestion")
            {
                currentIndex -= 5;
                HttpContext.Session.SetInt32("currentIndex", currentIndex);

                var shuffledQuestions = JsonSerializer.Deserialize<string[]>(HttpContext.Session.GetString("shuffledQuestions"));
                string[] question = { shuffledQuestions[currentIndex], shuffledQuestions[currentIndex + 1], shuffledQuestions[currentIndex + 2], shuffledQuestions[currentIndex + 3], shuffledQuestions[currentIndex + 4] }; ;
                TempData["Question"] = question;

                return RedirectToAction("Quiz", "AI");
            }
            else if (action == "NextQuestion")
            {
                currentIndex += 5;
                HttpContext.Session.SetInt32("currentIndex", currentIndex);

                var shuffledQuestions = JsonSerializer.Deserialize<string[]>(HttpContext.Session.GetString("shuffledQuestions"));
                string[] question = { shuffledQuestions[currentIndex], shuffledQuestions[currentIndex + 1], shuffledQuestions[currentIndex + 2], shuffledQuestions[currentIndex + 3], shuffledQuestions[currentIndex + 4] }; ;
                TempData["Question"] = question;

                return RedirectToAction("Quiz", "AI");
            }
            else if (action == "ReviewQuiz")
            {
                return RedirectToAction("Results");
            }
            else
            {
                Console.WriteLine("Error");
                ModelState.AddModelError("", "Invalid action.");
                return RedirectToAction("Study", "Home");
            }
        }

        [HttpGet]
        public IActionResult Results(string answer)
        {
            int[] results = new int[2];
            results[1] = HttpContext.Session.GetInt32("QuestionAmount") ?? 0; ;

            string[] answers = JsonSerializer.Deserialize<string[]>(HttpContext.Session.GetString("answers"));
            var storedQuestions = JsonSerializer.Deserialize<string[]>(HttpContext.Session.GetString("questions"));

            int numCorrect = 0;
            for (int i = 0; i < results[1]; i++)
            {
                if (storedQuestions[(i * 5) + 1] == answers[i])
                {
                    numCorrect++;
                }
            }

            results[0] = numCorrect;

            return View(model: results);
        }


        static string[] ShuffleArray(string[] array)
        {
            string[] shuffled = (string[])array.Clone();
            Random rng = new Random();
            int n = shuffled.Length;

            for (int i = n - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
            }

            return shuffled;
        }

    }
}
