using QuizMe.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuizMe.ViewModels
{
    public class CreateQuizViewModel
    {

        public List<Question> Questions { get; set; }

        public QuizRoom QuizRoom { get; set; }

        public Quiz Quiz { get; set; }

        
        public List<CorrectAnswer> CorrectAnswers { get; set; }

        public class CorrectAnswer {

            [Required(ErrorMessage ="Enter a correct option choice")]
            [Range(1, 4)]
            public int Option { get; set; }
        }
    }
}
