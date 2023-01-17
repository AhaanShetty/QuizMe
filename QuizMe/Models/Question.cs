using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizMe.Models
{
    public class Question
    {
        [Key]
        public string Id { get; set; }

        [Required(ErrorMessage = "Please enter the question text")]
        public string Text { get; set; }

        public string Quiz_Id { get; set; }

        [ForeignKey("Quiz_Id")]
        public QuizRoom QuizRoom { get; set; }

        public List<Answer> Answers { get; set; }
    }
}
