using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuizMe.Models
{
    public class Answer
    {
        [Key]
        public string Id { get; set; }

        public string Question_Id { get; set; }

        [ForeignKey("Question_Id")]
        public Question Question { get; set; }

        [Required(ErrorMessage = "Please enter the answer text")]
        public string Text { get; set; }

        [Required]
        public bool Is_Correct { get; set; }
    }
}
