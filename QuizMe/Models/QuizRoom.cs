using System.ComponentModel.DataAnnotations;

namespace QuizMe.Models
{
    public class QuizRoom
    {
        [Key]
        public string Quiz_id { get; set; }

        [Display(Name = "Enter quiz name")]
        [Required(ErrorMessage = "Please enter the quiz name")]
        public string Name { get; set; }

        [Display(Name = "Quiz Code")]
        [Required(ErrorMessage = "Please generate a quiz code")]
        public string Code { get; set; }

        [Display(Name = "Enter Quiz duration in minutes")]
        [Required(ErrorMessage = "Please enter the quiz duration")]
        public int Duration { get; set; }

        [Display(Name = "Enter Quiz Description")]
        [Required(ErrorMessage = "Please enter the quiz description")]
        public string Description { get; set; }

        [Display(Name = "Enter number of questions")]
        [Range(1,10)]
        [Required(ErrorMessage = "Please enter the number of questions")]
        public int NumberOfQuestions { get; set; }
    }
}
