using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QuizMe.Areas.Identity.Data;
using System;

namespace QuizMe.Controllers
{
    public class MainController : Controller
    {
        private readonly ILogger<MainController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        public MainController(ILogger<MainController> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _logger = logger;
            _userManager = userManager; 
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            try
            {
                bool isAuthenticated = User.Identity.IsAuthenticated;
                if(isAuthenticated)
                    return RedirectToAction("Index", "Home");
                return View();
            }
            catch (Exception e)
            {
                return View();
            }
            
        }
    }
}
