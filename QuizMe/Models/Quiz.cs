using QuizMe.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizMe.Models
{
    public class Quiz
    {
        [Key]
        public string Id { get; set; } 
        public string Quiz_Id { get; set; }

        [ForeignKey("Quiz_Id")]
        public QuizRoom QuizRoom { get; set; }

        public string User_Id { get; set; }

        [ForeignKey("User_Id")]
        public ApplicationUser ApplicationUser { get; set; }
        
    }
}
