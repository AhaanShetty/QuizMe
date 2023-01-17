using System.Collections.Generic;

namespace QuizMe.ViewModels
{
    public class QuizViewModel
    {

        public List<QuizDetails> Quizzes { get; set; }

        public class QuizDetails
        {
            public string Name { get; set; }
            public string QuizId { get; set; }
            public string Teacher_name { get; set; }
            public string Code { get; set; }
            public string Description { get; set; }
            public int Duration { get; set; }

            public int NumberOfQuestions { get; set; }

            public int lowest { get; set; }
            public int highest { get; set; }
            public float mean { get; set; }
            public int NumberOfStudents { get; set; }
        }

    }
}
