using Microsoft.AspNetCore.Identity;
using QuizMe.Areas.Identity.Data;
using System.Threading.Tasks;
using System;

namespace QuizMe.Utils
{
    public class CustomPasswordValidator<TUser>: IPasswordValidator<TUser> where TUser : ApplicationUser
    {
        public Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string password)
        {
            var passwordContainsUsername = password.Contains(user.UserName, StringComparison.OrdinalIgnoreCase);
            var passwordContainsEmail = password.Contains(user.Email, StringComparison.OrdinalIgnoreCase);
            var passwordContainsFirstName = password.Contains(user.FirstName, StringComparison.OrdinalIgnoreCase);
            var passwordContainsLastName = password.Contains(user.LastName, StringComparison.OrdinalIgnoreCase);
            if (passwordContainsUsername || passwordContainsEmail || passwordContainsFirstName || passwordContainsLastName)
            {
                return Task.FromResult(IdentityResult.Failed(new IdentityError
                {
                    Code = "PersonalInformationAsPassword",
                    Description = "Your pasword cannot contain your personal information"
                }));
            }
            return Task.FromResult(IdentityResult.Success);
        }
    }
}
