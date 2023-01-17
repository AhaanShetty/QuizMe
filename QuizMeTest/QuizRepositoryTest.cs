using System;
using Xunit;
using Moq;
using QuizMe.Controllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using QuizMe.Areas.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using QuizMe.Data;
using FakeItEasy;
using Microsoft.EntityFrameworkCore;
using QuizMe.Models;
using System.Linq;
using System.Collections.Generic;
using QuizMe.ViewModels;

namespace QuizMeTest
{
    public class QuizRepositoryTest
    {
        private UserManager<ApplicationUser> _userManager;
        private QuizRepository _quizRepository;
        private QuizMeContext _context;

        public QuizRepositoryTest()
        {
            _userManager = A.Fake<UserManager<ApplicationUser>>();
            _context = GetDBContext();
            _quizRepository = new QuizRepository(_context, _userManager);

            //Adding users
            for(int i=1;i <= 10; i++)
            {
                var user = new ApplicationUser()
                {
                    Id = "user" + i.ToString(),
                    UserName = "User" + i.ToString(),
                    FirstName = "Test",
                    LastName = "User",
                    PhoneNumber = "1223456789",
                    Email = "user" + i.ToString() + "@quizme.com"
                };
                _context.Users.Add(user);
            }
            _context.SaveChanges();

            //Adding quiz and questions
            for(int i = 1; i <= 4; i++)
            {
                var quizRoom = new QuizRoom()
                {
                    Quiz_id = "quiz" + i.ToString(),
                    Name = "Sample",
                    Description = "Sample",
                    Duration = i + 5,
                    NumberOfQuestions = 2,
                    Code = "1234"
                };
                _context.QuizRoom.Add(quizRoom);

                var quiz = new Quiz()
                {
                    Id = (i + 1).ToString(),
                    Quiz_Id = quizRoom.Quiz_id,
                    User_Id = "user" + i.ToString()
                };
                _context.Quiz.Add(quiz);

                for(int j = 1;j <= 2; j++)
                {
                    var question = new Question()
                    {
                        Id = "question"+ i.ToString() + j.ToString(),
                        Quiz_Id = quizRoom.Quiz_id,
                        Text = "Question " + j.ToString(),
                    };

                    for(int k=1;k<=4; k++)
                    {
                        var answer = new Answer()
                        {
                            Id = "answer" + i.ToString() + j.ToString() + k.ToString(),
                            Question_Id = question.Id,
                            Text = "Answer " + k.ToString(),
                            Is_Correct = (k == 2) ? true : false
                        };
                    }
                    _context.SaveChanges();
                }
                _context.SaveChanges();
            }
        }

        private QuizMeContext GetDBContext()
        {
            var options = new DbContextOptionsBuilder<QuizMeContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var databaseContext = new QuizMeContext(options);
            databaseContext.Database.EnsureCreated();
            return databaseContext;
        }

        [Theory]
        [InlineData("quiz1")]
        public void Test_GetQuizById(string Id)
        {
            //Act
            var result = _quizRepository.GetQuizById(Id);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<QuizRoom>(result);
        }

        [Fact]
        public void Test_GetAllQuizRooms()
        {
            //Act
            var result = _quizRepository.GetAllQuizRooms();

            //Assert
            Assert.NotNull(result);
            Assert.IsType<List<QuizRoom>>(result);
        }

        [Fact]
        public void Test_GetAllQuizzes()
        {
            //Act
            var result = _quizRepository.GetAllQuizzes();

            //Assert
            Assert.NotNull(result);
            Assert.IsType<List<Quiz>>(result);
        }

        [Theory]
        [InlineData("user1")]
        public void Test_GetAllQuizzesByUserId(string Id)
        {
            //Act
            var result = _quizRepository.GetAllQuizzes(Id);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<List<Quiz>>(result);
        }

        [Theory]
        [InlineData("quiz1")]
        public void Test_GetQuestionsByQuizId(string Quiz_Id)
        {
            //Act
            var result = _quizRepository.GetQuestionsByQuizId(Quiz_Id);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<List<Question>>(result);
        }

        [Theory]
        [InlineData("question11")]
        public void Test_GetAllAnswersByQuestionId(string question_id)
        {
            //Act
            var result = _quizRepository.GetAllAnswersByQuestionId(question_id);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<List<Answer>>(result);
        }

        [Fact]
        public void Test_CreateQuiz()
        {
            //Arrange
            var quizroom = new QuizRoom()
            {
                Quiz_id = "quiz5",
                Name = "Random",
                Description = "Random",
                Duration = 10,
                NumberOfQuestions = 1,
                Code = "1234",
            };
            var quiz = new Quiz()
            {
                Id = "6",
                Quiz_Id = "quiz5",
                User_Id = "user2"
            };
            var question = new Question()
            {
                Id = "question51",
                Quiz_Id = quizroom.Quiz_id,
                Text = "Question 1",
            };
            List<Answer> answer_list = new List<Answer>();
            for(int i=1; i<= 4; i++)
            {
                var answer = new Answer()
                {
                    Id = "answer51"+i.ToString(),
                    Text = "Answer " + i.ToString(),
                    Question_Id = question.Id,
                };
                answer_list.Add(answer);
            }
            question.Answers = answer_list;

            List<Question> questions_list = new List<Question>();
            questions_list.Add(question);

            var correct_answer = new CreateQuizViewModel.CorrectAnswer()
            {
                Option = 2
            };
            List<CreateQuizViewModel.CorrectAnswer> correct_answers_list = new List<CreateQuizViewModel.CorrectAnswer>();
            correct_answers_list.Add(correct_answer);

            var createquizmodel = new CreateQuizViewModel()
            {
                Questions = questions_list,
                QuizRoom = quizroom,
                Quiz = quiz,
                CorrectAnswers = correct_answers_list
            };

            var user = _context.Users.Find("user2");

            //Act
            var result = _quizRepository.CreateQuiz(createquizmodel, user);

            //Assert
            Assert.IsType<bool>(result);
        }

        [Theory]
        [InlineData("user3","quiz2")]
        public void Test_IsQuizSubmitted(string User_Id, string Quiz_Id)
        {
            //Act
            var result = _quizRepository.IsQuizSubmitted(User_Id, Quiz_Id);

            //Assert
            Assert.IsType<bool>(result);
        }

        [Theory]
        [InlineData("quiz2")]
        public void Test_GetQuizCode(string Quiz_Id)
        {
            //Act
            var result = _quizRepository.GetQuizCode(Quiz_Id);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<string>(result);
        }

        [Fact]
        public void Test_GetQuizzesForStudents()
        {
            //Act
            var result = _quizRepository.GetQuizzesForStudents().Result;

            //Assert
            Assert.NotNull(result);
            Assert.IsType<List<QuizViewModel.QuizDetails>>(result);
        }

        [Theory]
        [InlineData("user2")]
        public void Test_GetQuizzesForTeachers(string Teacher_Id)
        {
            //Act
            var result = _quizRepository.GetQuizzesForTeachers(Teacher_Id);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<List<QuizViewModel.QuizDetails>>(result);
        }

        [Theory]
        [InlineData(12)]
        public void Test_GenerateQuizCode(int size)
        {
            //Act
            var result = _quizRepository.GenerateQuizCode(size);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<string>(result);
        }

        [Theory]
        [InlineData("quiz2","user1")]
        public void Test_QuizIdExists(string quiz_id, string user_id)
        {
            //Act
            var result = _quizRepository.QuizIdExists(quiz_id, user_id);

            //Assert
            Assert.IsType<bool>(result);
        }

        [Theory]
        [InlineData("quiz2", "user1")]
        public void Test_DeleteQuiz(string quiz_id, string user_id)
        {
            //Act
            var result = _quizRepository.DeleteQuiz(quiz_id, user_id).Result;

            //Assert
            Assert.IsType<bool>(result);
        }

        [Theory]
        [InlineData("quiz2")]
        public void Test_GetQuizStats(string quiz_id)
        {
            //Act
            var result = _quizRepository.GetQuizStats(quiz_id);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<QuizViewModel.QuizDetails>(result);
        }

        [Theory]
        [InlineData("quiz2", "user1")]
        public void Test_GetCreateModel(string quiz_id, string user_id)
        {
            //Act
            var result = _quizRepository.GetCreateModel(quiz_id, user_id);

            //Assert
            Assert.IsType<CreateQuizViewModel>(result);
        }

        [Theory]
        [InlineData("user3")]
        public void Test_DeleteTeacherQuizzes(string Teacher_Id)
        {
            //Act
            var result = _quizRepository.DeleteTeacherQuizzes(Teacher_Id).Result;

            //Assert
            Assert.IsType<bool>(result);
        }

        [Theory]
        [InlineData("user3")]
        public void Test_DeleteStudentQuizzes(string user_id)
        {
            //Act
            var result = _quizRepository.DeleteStudentQuizzes(user_id);

            //Assert
            Assert.IsType<bool>(result);
        }

        [Theory]
        [InlineData("quiz3")]
        public void Test_IsCompleted(string quiz_id)
        {
            //Act
            var result = _quizRepository.IsCompleted(quiz_id);

            //Assert
            Assert.IsType<bool>(result);
        }

    }
}
