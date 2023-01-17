using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using QuizMe.Data;
using System;
using System.Threading.Tasks;

namespace QuizMe.Areas.Identity.Data
{
    public class UserRepository
    {
        private readonly QuizMeContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IServiceProvider _serviceProvider;
        public UserRepository(QuizMeContext context, UserManager<ApplicationUser> userManager, IServiceProvider serviceProvider)
        {
            _context = context; ;
            _userManager = userManager;
            _serviceProvider = serviceProvider;
        }

        public Task<ApplicationUser> GetUserByUsername(String username)
        {
            return _userManager.FindByNameAsync(username);
        }
        
        //Create a role for the registered user
        public async Task<bool> CreateRole(ApplicationUser user)
        {
            var roleManager = _serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            Task<IdentityResult> roleResult;
            if (user == null) return false;
            else
            {
                string assignedRole = user.Role;
                if(assignedRole == "Student")
                {
                    var hasStudentRole = await roleManager.RoleExistsAsync("Student");
                    if (!hasStudentRole)
                    {
                        roleResult = roleManager.CreateAsync(new IdentityRole("Student"));
                    }
                    Task<IdentityResult> newUserRole = _userManager.AddToRoleAsync(user, "Student");
                }
                else if(assignedRole == "Teacher")
                {
                    var hasTeacherRole = await roleManager.RoleExistsAsync("Teacher");
                    if (!hasTeacherRole)
                    {
                        roleResult = roleManager.CreateAsync(new IdentityRole("Teacher"));
                    }
                    Task<IdentityResult> newUserRole = _userManager.AddToRoleAsync(user, "Teacher");
                }
                return true;
            }
            
        }
    }
}
