using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QuizMe.Areas.Identity.Data;
using System.Threading.Tasks;
using QuizMe.ViewModels;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace QuizMe.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly QuizRepository _quizRepository;

        public AdminController(ILogger<AdminController> logger, UserManager<ApplicationUser> userManager, QuizRepository quizRepository)
        {
            _logger = logger;
            _userManager = userManager;
            _quizRepository = quizRepository;
        }

        //Admin landing page
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            return RedirectToAction("Users","Admin");
        }

        //Admin list users page
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Users()
        {
            _logger.LogInformation("Admin " + _userManager.GetUserName(User) + " has viewed the Users page");
            //Get all users that are not admin
            IEnumerable<ApplicationUser> all_users = _userManager.Users.Where(u => u.Role != "Admin");
            return View(all_users);
        }


        //Delete a particular user
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string User_Id)
        {
            if(User_Id == null)
            {
                return NotFound();
            }
            //Find user by Id
            var user = await _userManager.FindByIdAsync(User_Id);

            //If null then return NotFound()
            if(user == null)
            {
                return NotFound();
            }

            string role = user.Role;
            var delete_status = false;

            //Check if the user is a student or a teacher
            if (role == "Teacher")
            {
                //If teacher then first delete all teacher quizzes, questions and answers
                delete_status = await _quizRepository.DeleteTeacherQuizzes(User_Id);
            }else if(role == "Student")
            {
                //If student then firs delete all completed quizzes
                delete_status = _quizRepository.DeleteStudentQuizzes(User_Id);
            }
            else { delete_status = true; }

            //Delete User
            var result = await _userManager.DeleteAsync(user);

            //If all deletion was succesfully
            if (result.Succeeded || delete_status)
            {
                _logger.LogInformation("Admin " + _userManager.GetUserName(User) + " has deleted the user with User_Id " + User_Id);
                return RedirectToAction("Index", "Home");
            }

            //if any errors occured
            foreach(var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return RedirectToAction("Users", "Admin");
        }


        //Post request to handle the approval of teachers
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(string Teacher_Id)
        {
            //Check if teacher_id is null or not
            if(Teacher_Id == null)
            {
                return NotFound();
            }
            //Find user by Id
            var user = await _userManager.FindByIdAsync(Teacher_Id);
            if(user == null)
            {
                return NotFound();
            }
            //Approve teacher
            user.isAdminApproved = true;

            //Update the user record
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation("Admin " + _userManager.GetUserName(User) + " has approved the request for teacher with User_Id " + Teacher_Id);
                return RedirectToAction("Index", "Home");
            }
            //Display errors if any
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return RedirectToAction("Users", "Admin");
        }
    }
}
