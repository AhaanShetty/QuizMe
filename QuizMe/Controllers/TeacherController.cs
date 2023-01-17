using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuizMe.Areas.Identity.Data;
using QuizMe.Models;
using QuizMe.ViewModels;
using System;
using QuizMe.Utils;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;

namespace QuizMe.Controllers
{
    public class TeacherController : Controller
    {

        private readonly ILogger<TeacherController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly QuizRepository _quizRepository;
        private readonly UserRepository _userRepository;
        private readonly IDataProtectionProvider _protectionProvider;
        private readonly IConfiguration _configuration;

        public TeacherController(ILogger<TeacherController> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, QuizRepository quizRepository, UserRepository userRepository, IDataProtectionProvider dataProtectionProvider, IConfiguration configuration)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _quizRepository = quizRepository;
            _userRepository = userRepository;
            _protectionProvider = dataProtectionProvider;
            _configuration = configuration;
        }

        //Teacher landing page
        [Authorize(Roles = "Teacher")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            //Get current logged in user
            var user = await _userManager.GetUserAsync(User);
            //If user is null then redirect them to the login page
            if(user == null)
            {
                return Redirect("/Identity/Account/Login");
            }
            //Get all the quizzes created by the current logged in teacher
            var quiz_list = _quizRepository.GetQuizzesForTeachers(user.Id);
            if(quiz_list.Count == 0)
            {
                ViewBag.Message = "You haven't created any quizzes";
                return View();
            }

            //Store the quiz details in QuizViewModel model
            foreach(var quiz in quiz_list)
            {
                var quiz_stat = _quizRepository.GetQuizStats(quiz.QuizId);
                quiz.lowest = quiz_stat.lowest;
                quiz.highest = quiz_stat.highest;
                quiz.mean = quiz_stat.mean;
                quiz.NumberOfStudents = quiz_stat.NumberOfStudents;
            }
            return View(new QuizViewModel()
            {
                Quizzes = quiz_list,
            });
        }

        //Generate a quiz code everytime the teacher clicks on the generate code button
        [Authorize(Roles = "Teacher")]
        [HttpGet]
        public JsonResult GenerateCode()
        {
            string code = _quizRepository.GenerateQuizCode(12);
            _logger.LogInformation("Teacher " + _userManager.GetUserName(User) + " has generated a code: " + code);
            return Json(new { Code = code});
        }

        //Provide basic quiz details
        [Authorize(Roles = "Teacher")]
        [HttpGet]
        public IActionResult Generate(CreateQuizViewModel createQuizModel)
        {

            return View(createQuizModel);
        }

        [Authorize(Roles = "Teacher")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult Create(CreateQuizViewModel createQuizModel)
        {
            if (ModelState.IsValid)
            {
                //Check if number of questions is between 1-10
                if (1 <= createQuizModel.QuizRoom.NumberOfQuestions && createQuizModel.QuizRoom.NumberOfQuestions <= 10)
                {
                    
                    var quiz_model = new CreateQuizViewModel()
                    {
                        QuizRoom = createQuizModel.QuizRoom,
                    };

                    DataProtectionProviderEncryption encryptor = new DataProtectionProviderEncryption(_protectionProvider, _configuration["Encryption_Key"]);
                    var encrypted_code = encryptor.encrypt(quiz_model.QuizRoom.Code);
                    Console.WriteLine("Encrypted code is " + encrypted_code);
                    Console.WriteLine("Decrypted Code is " + encryptor.decrypt(encrypted_code));

                    _logger.LogInformation("Teacher " + _userManager.GetUserName(User) + " has created quiz details");
                    return View(quiz_model);
                }
                else
                {
                    TempData["InvalidQuestionCount"] = "Invalid number of questions";
                    return RedirectToAction("Generate", "Teacher");
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(x => x.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }
            }
            
            return RedirectToAction("Generate", "Teacher");
        }

        //Save quiz with questions and answers
        [Authorize(Roles = "Teacher")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> SaveQuiz(CreateQuizViewModel createQuizModel)
        {
            
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Redirect("Identity/Account/Login");
            }
            if (ModelState.IsValid)
            {
                //var quiz_room = createQuizModel.QuizRoom;
                
                //quiz_room.Code = DataProtectionProviderEncryption.
                var quiz_model = new CreateQuizViewModel()
                {
                    Questions = createQuizModel.Questions,
                    QuizRoom = createQuizModel.QuizRoom,
                    CorrectAnswers = createQuizModel.CorrectAnswers
                };             
                var result = _quizRepository.CreateQuiz(quiz_model, user);
                _logger.LogInformation("Teacher " + _userManager.GetUserName(User) + " has created a quiz with Title " + quiz_model.QuizRoom.Name);

                return RedirectToAction("Index", "Teacher");
            }
            return View("Create", createQuizModel);
        }
        
        //Delete a quiz by its ID
        [HttpPost]
        [Authorize(Roles = "Teacher")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string quiz_id)
        {
            Console.Write(quiz_id);
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Redirect("Identity/Account/Login");
            }
            var delete_status = _quizRepository.DeleteQuiz(quiz_id,user.Id);
            _logger.LogInformation("Teacher " + _userManager.GetUserName(User) + " has deleted a quiz with Id " + quiz_id);
            return Ok(delete_status);
            //return RedirectToAction("Index","Teacher");

        }

        //View Quiz on clicking view quiz
        [HttpGet]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Quiz(string Id)
        {
            if (Id == null) return RedirectToAction("Index", "Teacher");
            
            var quiz_room = _quizRepository.GetQuizById(Id);

            if(quiz_room == null) return RedirectToAction("Index", "Teacher");

            //Check if any user has completed the quiz before the teacher edits
            var completed = _quizRepository.IsCompleted(Id);
            if (completed)
            {
                TempData["Error"] = "You cannot edit this quiz. A student has already completed it";
                return RedirectToAction("Index", "Teacher");
            }

            //Get Teacher Id
            var user = await _userManager.GetUserAsync(User);
            //Build model with fields from DB
            CreateQuizViewModel EditModel = _quizRepository.GetCreateModel(Id, user.Id);

            return View(EditModel);
        }


        //Edit quiz withe new data
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Edit(CreateQuizViewModel editQuizModel)
        {
            var user = await _userManager.GetUserAsync(User);
            
            if (user == null)
            {
                return Redirect("Identity/Account/Login");
            }
            
            if (ModelState.IsValid)
            {
                var quiz_room = editQuizModel.QuizRoom;
                
                quiz_room.NumberOfQuestions = _quizRepository.GetQuizById(quiz_room.Quiz_id).NumberOfQuestions;
                var quiz_model = new CreateQuizViewModel()
                {
                    Questions = editQuizModel.Questions,
                    QuizRoom = quiz_room,
                    CorrectAnswers = editQuizModel.CorrectAnswers
                };
                var result = _quizRepository.EditQuiz(quiz_model, user);
                if (result)
                {
                    TempData["EditedQuiz"] = "You have edited a quiz";
                    _logger.LogInformation("Teacher " + _userManager.GetUserName(User) + " has edited quiz with Id " + quiz_room.Quiz_id);
                    return RedirectToAction("Index", "Teacher");
                }
                
                return RedirectToAction("Index", "Teacher");
            }
            else
            {
                var errors = ModelState.Values.SelectMany(x => x.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }
            }
            return RedirectToAction("Quiz", "Teacher",new { Id = editQuizModel.QuizRoom.Quiz_id});
        }
    }
}
