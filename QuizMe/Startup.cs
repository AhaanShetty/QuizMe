using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using QuizMe.Areas.Identity.Data;
using QuizMe.Data;
using QuizMe.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;

namespace QuizMe
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddDataProtection();
            //Set password options
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 12;
                options.Password.RequiredUniqueChars = 4;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromHours(24);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

                //Unique Email
                options.User.RequireUniqueEmail = true;

                //Use goattotpprovider
                options.Tokens.PasswordResetTokenProvider = typeof(QuizMeTotpSecurityStampBasedTokenProvider<ApplicationUser>).Name.Split('`')[0];
            });

            //Configure Cookie
            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

            services.AddDistributedMemoryCache();

            //services.AddSession(options =>
            //{
            //    options.Cookie.HttpOnly = true;
            //    options.IdleTimeout = TimeSpan.FromMinutes(30);
            //});

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };

            Console.WriteLine(Configuration["Encryption_Key"]);

            services.AddScoped<QuizRepository>();
            services.AddScoped<UserRepository>();

            //Implementing pbkdf2 with 100000 iterations
            services.Configure<PasswordHasherOptions>(option =>
            {
                option.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
                option.IterationCount = 100000;
            });
            Console.WriteLine("IN the startup configure service method");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            Console.WriteLine("IN the startup configure method");

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            //Customizing security headers
            //app.Use((context, next) =>
            //{
            //    var headers = context.Response.Headers;

            //    //Adding necessary headers
            //    headers.Add("X-Frame-Options", "DENY");
            //    headers.Add("X-XSS-Protection", "1; mode=block");
            //    headers.Add("X-Content-Type-Options", "nosniff");
            //    headers.Add("Referrer-Policy", "no-referrer");
            //    //headers.Add("Content-Security-Policy", "default - src 'none'; script - src 'self'; connect - src 'self'; img - src 'self'; style - src 'self'; frame - ancestors 'self'; form - action 'self'");

            //    //Removing headers
            //    headers.Remove("X-Powered-By");
            //    headers.Remove("x-aspnet-version");
            //    headers.Remove("Server");

            //    return next();
            //});

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Main}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });


            //Uncomment this if you want to create an administrator
            //CreateAdmin(serviceProvider);
        }

        private void CreateAdmin(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            Task<IdentityResult> roleResult;
            string email = Configuration["Admin:Email"];
            Console.WriteLine("Inside create admin method");
            //Check that there is an Admin role and create if not
            Task<bool> hasAdminRole = roleManager.RoleExistsAsync("Admin");
            hasAdminRole.Wait();

            if (!hasAdminRole.Result)
            {
                roleResult = roleManager.CreateAsync(new IdentityRole("Admin"));
                roleResult.Wait();
            }

            //Check if the admin user exists and create it if not
            //Add to the Admin role

            Task<ApplicationUser> adminUser = userManager.FindByEmailAsync(email);
            adminUser.Wait();
            if (adminUser.Result == null)
            {
                ApplicationUser admin = new ApplicationUser();
                admin.Email = Configuration["Admin:Email"];
                admin.UserName = Configuration["Admin:Username"];
                admin.FirstName = "Admin";
                admin.LastName = "Account";
                admin.EmailConfirmed = true;
                admin.Role = "Admin";
                string password = Configuration["Admin:Password"];
                Task<IdentityResult> newUser = userManager.CreateAsync(admin, password);
                newUser.Wait();

                if (newUser.Result.Succeeded)
                {
                    Console.WriteLine("Created Admin user");
                    Task<IdentityResult> newUserRole = userManager.AddToRoleAsync(admin, "Admin");
                    newUserRole.Wait();
                }
            }

        }
    }
}
