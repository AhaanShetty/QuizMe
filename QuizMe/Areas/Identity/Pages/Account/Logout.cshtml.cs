using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using QuizMe.Areas.Identity.Data;

namespace QuizMe.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            var user = await _userManager.GetUserAsync(User);
            await _signInManager.SignOutAsync();

            //HttpContext.Session.Clear();

            //var keys = HttpContext.Session.Keys;
            var cookies = Request.Cookies.Keys;

            //Delete Cookies
            foreach(var cookie in cookies)
            {
                Response.Cookies.Delete(cookie); 
            }
            _logger.LogInformation("User "+ user.UserName + " logged out.");
            if (returnUrl != null)
            {
                return RedirectToAction("Index","Main");
            }
            else
            {
                return RedirectToPage();
            }
        }
    }
}
