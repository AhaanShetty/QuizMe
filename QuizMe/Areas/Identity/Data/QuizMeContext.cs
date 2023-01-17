using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuizMe.Areas.Identity.Data;
using QuizMe.Models;

namespace QuizMe.Data
{
    public class QuizMeContext : IdentityDbContext<ApplicationUser>
    {
        public QuizMeContext(DbContextOptions<QuizMeContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder)
            
            
        }
        public virtual DbSet<Question> Question { get; set; }

        public virtual DbSet<Answer> Answer { get; set; }

        public virtual DbSet<Quiz> Quiz { get; set; }

        public virtual DbSet<QuizRoom> QuizRoom { get; set; }

        public virtual DbSet<UserInRoom> UserInRoom { get; set; }

        public virtual DbSet<UserQuestionHistory> UserQuestionHistory { get; set; }

        public virtual DbSet<UserAnswerHistory> UserAnswerHistory { get; set; }


    }
}
