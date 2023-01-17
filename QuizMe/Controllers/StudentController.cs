using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QuizMe.Areas.Identity.Data;
using QuizMe.Data;
using QuizMe.Models;
using QuizMe.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace QuizMe.Controllers
{
    public class StudentController : Controller
    {

        private readonly ILogger<StudentController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly QuizRepository _quizRepository;
        private readonly UserRepository _userRepository;
        private readonly QuizMeContext _context;

        public StudentController(ILogger<StudentController> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, QuizRepository quizRepository, UserRepository userRepository, QuizMeContext quizMeContext)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _quizRepository = quizRepository;
            _userRepository = userRepository;
            _context = quizMeContext;
        }

        //Student landing page
        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Index()
        {
            //Get all quizzes
            var quiz_list = await _quizRepository.GetQuizzesForStudents();
            _logger.LogInformation("Student " + _userManager.GetUserName(User) + " has viewed their home page ");
            return View(new QuizViewModel()
            {
                Quizzes = quiz_list,
            });   
        }

        //Submit quiz code and perform check
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Index(string Id, string Code)
        {
            ViewBag.Info = null;
            ViewBag.Error = null;
            var quiz_list = await _quizRepository.GetQuizzesForStudents();
            //Check if code is null or not
            if (Code == null)
            {
                ViewBag.Info = null;
                ViewBag.Error = "Please enter a code";
                return View(new QuizViewModel()
                {
                    Quizzes = quiz_list,
                });
            }

            //Get code for a particular quiz
            string quiz_code = _quizRepository.GetQuizCode(Id);
            string user_id = (await _userManager.GetUserAsync(User))?.Id;

            if (quiz_code != null)
            {
                //Check if entered code is the same as the one in the database
                var code_result = Code.Equals(quiz_code, StringComparison.Ordinal);

                //Check if the user has already completed the quiz
                var is_Submitted = _quizRepository.IsQuizSubmitted(user_id, Id);

                //If code is correct
                if (code_result == true)
                {
                    //If quiz is already completed
                    if (is_Submitted)
                    {
                        ViewBag.Info = null;
                        ViewBag.Error = "You have already given this quiz";
                        return View(new QuizViewModel()
                        {
                            Quizzes = quiz_list,
                        });
                    }
                    _logger.LogInformation("Student " + _userManager.GetUserName(User) + " has entered the room with Id: " + Id + " and Code " + Code);
                    return RedirectToAction("Room", "Student", new { Id = Id });
                }
            }
            ViewBag.Info = null;
            ViewBag.Error = "Invalid Quiz Code";
            _logger.LogError("Student " + _userManager.GetUserName(User) + " has submitted an incorrect code for the quiz");
            return View(new QuizViewModel()
            {
                Quizzes = quiz_list,
            });
        }

        //Entering a quiz room
        [HttpGet]
        [Authorize(Roles = "Student")]
        public IActionResult Room(String Id)
        {

            if(Id == null)
            {
                _logger.LogError("Quiz Id is null");
                return NotFound();
            }
            //Get all questions for a quiz
            var question_list = _quizRepository.GetQuestionsByQuizId(Id);
            if(question_list.Count() == 0)
            {
                _logger.LogError("Invalid Quiz Id");
                return NotFound();
            }

            foreach (var question in question_list)
            {
                string question_id = question.Id;
                //Get all answers for a particular question id
                question.Answers = _quizRepository.GetAllAnswersByQuestionId(question_id);
            }
            //Get quiz details
            var quiz_info = _quizRepository.GetQuizById(Id);
            ViewBag.Quiz_Details = new QuizViewModel.QuizDetails()
            {
                Name = quiz_info.Name,
                QuizId = quiz_info.Quiz_id,
                Description = quiz_info.Description,
                Duration = quiz_info.Duration,
            };
            
            return View(question_list);
        }

        //Submit a quiz
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Submit(IFormCollection formData)
        {
            string Quiz_Id = formData["Quiz_Id"];
            if(Quiz_Id == null)
            {
                _logger.LogError("Did not enter correct value for Quiz Id");
            }
            var questions = _quizRepository.GetQuestionsByQuizId(Quiz_Id);
            if(questions.Count() == 0)
            {
                _logger.LogError("Could not find questions for quiz with Id " + Quiz_Id);
            }
            int count = questions.Count;
            int score = 0;
            foreach(var question in questions)
            {
                var answer_field = "Question#" + question.Id;
                var answer_id = formData[answer_field].ToString().Split("#")[1];
                //Check if the answer is correct
                var isCorrect = _context.Answer.Single(a => a.Id == answer_id).Is_Correct;
                if (isCorrect)
                {
                    score++;
                }
            }
            string user_id = (await _userManager.GetUserAsync(User))?.Id;
            var new_user_submission = new UserInRoom()
            {
                Id = Guid.NewGuid().ToString(),
                User_Id = user_id,
                Quiz_Room_Id = Quiz_Id,
                Score = score,
            };
            _context.UserInRoom.Add(new_user_submission);
            _context.SaveChanges();
            _logger.LogInformation("Student " + _userManager.GetUserName(User) + " has completed the quiz with Id " + Quiz_Id);
            return RedirectToAction("Quizzes", "Student");
        }

        //Get all student quizzes
        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Quizzes() {
            string user_id = (await _userManager.GetUserAsync(User))?.Id;
            _logger.LogInformation("Student " + _userManager.GetUserName(User) + " has viewed their my quizzes page");
            var AllQuizzesByUser = _context.UserInRoom.Where(uir => uir.User_Id == user_id);
            List<CompletedQuizzesViewModel> quizzes = new List<CompletedQuizzesViewModel>();
            if (AllQuizzesByUser.Count() == 0)
            {
                ViewBag.Message = "You haven't given any quizzes";
                return View();
            }
            foreach(var quiz in AllQuizzesByUser)
            {
                string Quiz_Id = quiz.Quiz_Room_Id;
                var quiz_room = _quizRepository.GetQuizById(Quiz_Id);
                var quiz_given = new CompletedQuizzesViewModel()
                {
                    quizRoom = quiz_room,
                    score = quiz.Score
                };
                quizzes.Add(quiz_given);
            }
            return View(quizzes);
        }
    }
    
}
