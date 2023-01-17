using QuizMe.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizMe.Models
{
    public class UserAnswerHistory
    {
        [Key]
        public string Id { get; set; }
        public string User_Id { get; set; }

        [ForeignKey("User_Id")]
        public ApplicationUser ApplicationUser { get; set; }

        public string Quiz_Id { get; set; }

        [ForeignKey("Quiz_Id")]
        public QuizRoom QuizRoom { get; set; }

        public string Question_Id { get; set; }

        [ForeignKey("Quiz_Id")]
        public Question Question { get; set; }

        public string Answer_Id { get; set; }

        [ForeignKey("Answer_Id")]
        public Answer Answer { get; set; }
    }
}

