using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuizMe.Areas.Identity.Data;
using QuizMe.Data;
using QuizMe.Utils;

[assembly: HostingStartup(typeof(QuizMe.Areas.Identity.IdentityHostingStartup))]
namespace QuizMe.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<QuizMeContext>(options =>
                    options.UseSqlite(
                        context.Configuration.GetConnectionString("QuizMeContextConnection")));

                services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<QuizMeContext>()
                .AddDefaultTokenProviders()
                .AddTokenProvider<QuizMeTotpSecurityStampBasedTokenProvider<ApplicationUser>>("QuizMeTotpSecurityStampBasedTokenProvider")
                .AddPasswordValidator<CustomPasswordValidator<ApplicationUser>>(); ;
            });
        }
    }
}