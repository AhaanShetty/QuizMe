using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Newtonsoft.Json.Linq;
using QuizMe.Data;
using QuizMe.Models;
using QuizMe.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace QuizMe.Areas.Identity.Data
{
    public class QuizRepository
    {
        private readonly QuizMeContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

        public QuizRepository(QuizMeContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        //Get a quiz by its ID
        public QuizRoom GetQuizById(string quiz_id)
        {
            return _context.QuizRoom.Single(p => p.Quiz_id == quiz_id);
        }

        //Get all quiz rooms
        public List<QuizRoom> GetAllQuizRooms()
        {
            return _context.QuizRoom.ToList();
        }

        //Get all quizzes
        public List<Quiz> GetAllQuizzes()
        {
            return _context.Quiz.ToList();
        }

        //Get quizzess by a particular teacher
        public List<Quiz> GetAllQuizzes(string User_Id)
        {
            return _context.Quiz.Where(q => q.User_Id == User_Id).ToList();
        }

        //Get questions for a particular quiz
        public List<Question> GetQuestionsByQuizId(string QuizId)
        {
            List<Question> question_list = new List<Question>();
            question_list = _context.Question.Where(q => q.Quiz_Id.Equals(QuizId)).ToList();
            return question_list;
        }
        
        //Get Answers using Question Id
        public List<Answer> GetAllAnswersByQuestionId(string Question_Id)
        {
            List<Answer> answer_list = new List<Answer>();
            answer_list = _context.Answer.Where(a => a.Question_Id.Equals(Question_Id)).ToList();
            return answer_list;
        }

        //Create a Quiz
        public bool CreateQuiz(CreateQuizViewModel quizViewModel, ApplicationUser user)
        {
            //Add quiz room
            var room = new QuizRoom()
            {
                Quiz_id = Guid.NewGuid().ToString(),
                Name = quizViewModel.QuizRoom.Name,
                Code = quizViewModel.QuizRoom.Code,
                Description = quizViewModel.QuizRoom.Description,
                Duration = quizViewModel.QuizRoom.Duration,
                NumberOfQuestions = quizViewModel.QuizRoom.NumberOfQuestions
            };
            _context.QuizRoom.Add(room);
            _context.SaveChanges();
            string Quiz_Id = room.Quiz_id;
            var quiz = new Quiz()
            {
                Id = Guid.NewGuid().ToString(),
                Quiz_Id = Quiz_Id,
                User_Id = user.Id
            };
            _context.Quiz.Add(quiz);
            _context.SaveChanges();
            List<int> correct_answer_list = new List<int>();
            foreach(var correct_answer in quizViewModel.CorrectAnswers){
                correct_answer_list.Add(correct_answer.Option);
            }
            int i = 0;
            foreach(var question in quizViewModel.Questions)
            {
                var new_question = new Question()
                {
                    Id = Guid.NewGuid().ToString(),
                    Quiz_Id = Quiz_Id,
                    Text = question.Text,
                };
                _context.Question.Add(new_question);
                _context.SaveChanges();
                int j = 1;
                foreach(var answer in question.Answers)
                {
                    var new_answer = new Answer()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Question_Id = new_question.Id,
                        Text = answer.Text,
                        Is_Correct = correct_answer_list[i] == j ? true: false,
                    };
                    _context.Answer.Add(new_answer);
                    _context.SaveChanges();
                    j++;
                };
                i++;
            }
            return true;
           
        }

        //Check if the quiz is submitted by a user
        public bool IsQuizSubmitted(string User_Id, string Quiz_Id)
        {
            var result = _context.UserInRoom.Any(uir => uir.User_Id == User_Id && uir.Quiz_Room_Id == Quiz_Id);
            if (result) return true;
            return false;
        }

        //Get code for a particular quiz
        public string GetQuizCode(string quiz_id)
        {
            var quiz_info =  _context.QuizRoom.Single(p => p.Quiz_id == quiz_id);
            return quiz_info.Code;
        }

        //Get all quizzes
        public async Task<List<QuizViewModel.QuizDetails>> GetQuizzesForStudents()
        {
            var all_quiz_rooms = GetAllQuizRooms();
            var all_quizzes = GetAllQuizzes();
            List<QuizViewModel.QuizDetails> quiz_list = new List<QuizViewModel.QuizDetails>();
            foreach (var quiz_room in all_quiz_rooms)
            {
                foreach (var quiz in all_quizzes)
                {
                    if (quiz_room.Quiz_id == quiz.Quiz_Id)
                    {
                        var user = await _userManager.FindByIdAsync(quiz.User_Id);
                        var fullname = user.FirstName + " " + user.LastName;
                        var temp_quiz = new QuizViewModel.QuizDetails()
                        {
                            Name = quiz_room.Name,
                            QuizId = quiz_room.Quiz_id,
                            Teacher_name = fullname,
                            Description = quiz_room.Description,
                            Duration = quiz_room.Duration,
                            NumberOfQuestions = quiz_room.NumberOfQuestions
                        };
                        quiz_list.Add(temp_quiz);
                    }
                }
            }
            
            return quiz_list;
        }

        //Get all quizzes for a particular teacher
        public List<QuizViewModel.QuizDetails> GetQuizzesForTeachers(string Teacher_Id)
        {
            var all_quizzes = GetAllQuizzes(Teacher_Id);
            var all_quiz_rooms = GetAllQuizRooms();
            List<QuizViewModel.QuizDetails> quiz_list = new List<QuizViewModel.QuizDetails>();
            foreach (var quiz_room in all_quiz_rooms)
            {
                foreach (var quiz in all_quizzes)
                {
                    if (quiz_room.Quiz_id == quiz.Quiz_Id)
                    {
                        var temp_quiz = new QuizViewModel.QuizDetails
                        {
                            Name = quiz_room.Name,
                            QuizId = quiz_room.Quiz_id,
                            Code = quiz_room.Code,
                            Description = quiz_room.Description,
                            Duration = quiz_room.Duration,
                            NumberOfQuestions = quiz_room.NumberOfQuestions
                        };
                        quiz_list.Add(temp_quiz);
                    }
                }
            }
            
            return quiz_list;
        }

        //Generate a random quiz code
        public string GenerateQuizCode(int size)
        {
            //Using RnadomNumberGenerator Class to generate code
            byte[] data = new byte[4 * size];
            using (var crypto = RandomNumberGenerator.Create())
            {
                crypto.GetBytes(data);
            }
            StringBuilder result = new StringBuilder(size);
            for (int i = 0; i < size; i++)
            {
                var rnd = BitConverter.ToUInt32(data, i * 4);
                var idx = rnd % chars.Length;

                result.Append(chars[idx]);
            }

            return result.ToString();
        }

        //Check if a quiz exists
        public bool QuizIdExists(string quiz_id, string user_id)
        {
            return _context.QuizRoom.Any(c => c.Quiz_id == quiz_id);
        }

        //Delete a quiz by its ID
        public async Task<bool> DeleteQuiz(string Id, string user_id)
        {
            //Delete Quiz room
            var quiz_room = await _context.QuizRoom.FindAsync(Id);
            if (quiz_room == null) return false;
            

            //Delete the quiz created by the user
            var quiz = _context.Quiz.Where(q => q.Quiz_Id == Id && q.User_Id == user_id).FirstOrDefault();
            if (quiz == null)
                return false;
            

            //Delete Questions
            var questions = _context.Question.Where(q => q.Quiz_Id == Id).ToList();
            if (questions.Count() == 0) return false;
            

            //Delete Answers
            foreach(var question in questions)
            {
                var answers = _context.Answer.Where(a => a.Question_Id == question.Id).ToList();
                if (answers.Count() == 0) return false;
                //Delete Answers
                _context.Answer.RemoveRange(answers);
                _context.SaveChanges();
            }

            //Delete Questions
            _context.Question.RemoveRange(questions);
            _context.SaveChanges();

            //Delete All user records that have given the quiz
            var completed_quiz_by_users = _context.UserInRoom.Where(uir => uir.Quiz_Room_Id == Id).ToList();
            _context.UserInRoom.RemoveRange(completed_quiz_by_users);
            _context.SaveChanges();

            //Delete Quiz Room
            _context.QuizRoom.Remove(quiz_room);
            _context.SaveChanges();

            //Delete Quiz
            _context.Quiz.Remove(quiz);
            _context.SaveChanges();

            

            return true;
        }

        //Calculate quiz statistics such as lowest, higest, mean etc.
        public QuizViewModel.QuizDetails GetQuizStats(string Id)
        {
            var completed_quizzes = _context.UserInRoom.Where(uir => uir.Quiz_Room_Id == Id);
            if (completed_quizzes.Count() == 0)
            {
                return new QuizViewModel.QuizDetails()
                {
                    NumberOfStudents = 0,
                    lowest = 0,
                    highest = 0,
                    mean = 0
                };
            }
            int lowest = 20, highest=-1, count = completed_quizzes.Count(), total = 0;
            float mean;
            foreach(var quiz in completed_quizzes)
            {
                if(quiz.Score < lowest) lowest = quiz.Score;
                if(quiz.Score > highest) highest = quiz.Score;
                total += quiz.Score;
            }
            mean = (float)total / count;
            var quiz_stats = new QuizViewModel.QuizDetails()
            {
                NumberOfStudents = count,
                lowest = lowest,
                highest = highest,
                mean = mean
            };
            return quiz_stats;
        }

        //Get the create quiz model
        public CreateQuizViewModel GetCreateModel(string Id, string User_Id)
        {
            CreateQuizViewModel viewModel = new CreateQuizViewModel();
            var quiz_room = GetQuizById(Id);
            var QuestionsforQuiz = GetQuestionsByQuizId(Id);
            List<CreateQuizViewModel.CorrectAnswer> correct_answers_list = new List<CreateQuizViewModel.CorrectAnswer>();
            //List<Question> question_list = new List<Question>();
            foreach(var question in QuestionsforQuiz)
            {
                var question_id = question.Id;
                var AnswersForQuestion = GetAllAnswersByQuestionId(question_id);
                question.Answers = AnswersForQuestion;
                int i = 1;
                foreach(var answer in AnswersForQuestion)
                {
                    if (answer.Is_Correct)
                    {
                        CreateQuizViewModel.CorrectAnswer correct_answer = new CreateQuizViewModel.CorrectAnswer()
                        {
                            Option = i
                        };
                        correct_answers_list.Add(correct_answer);
                    }
                    i++;
                }
               
                //question_list.Add(question);
            }
            viewModel.Questions = QuestionsforQuiz;
            viewModel.CorrectAnswers = correct_answers_list;
            viewModel.QuizRoom = quiz_room;
            viewModel.Quiz = _context.Quiz.Where(q => q.Quiz_Id == Id && q.User_Id == User_Id).FirstOrDefault();
            return viewModel;
        }

        //Get the edit quiz model
        public bool EditQuiz(CreateQuizViewModel editViewModel, ApplicationUser user)
        {
            //Edit Quiz Room
            var room = _context.QuizRoom.Find(editViewModel.QuizRoom.Quiz_id);
            Console.WriteLine(room.Quiz_id);
            room.Name = editViewModel.QuizRoom.Name;
            room.Code = editViewModel.QuizRoom.Code;
            room.Description = editViewModel.QuizRoom.Description;
            room.Duration = editViewModel.QuizRoom.Duration;
           
            _context.QuizRoom.Update(room);
            _context.SaveChanges();

            //Update Quiz
            
            List<int> correct_answer_list = new List<int>();
            foreach (var correct_answer in editViewModel.CorrectAnswers)
            {
                correct_answer_list.Add(correct_answer.Option);
            }
            int i = 0;

            foreach (var question in editViewModel.Questions)
            {
                //Update Question
                var retrieved_question = _context.Question.Where(q => q.Id == question.Id && q.Quiz_Id == editViewModel.QuizRoom.Quiz_id).FirstOrDefault();
                retrieved_question.Text = question.Text;
                
                _context.Question.Update(retrieved_question);
                _context.SaveChanges();

                int j = 1;
                foreach (var answer in question.Answers)
                {
                    //Update Answer
                    var retrieved_answer = _context.Answer.Where(a => a.Id == answer.Id && a.Question_Id == question.Id).FirstOrDefault();
                    retrieved_answer.Text = answer.Text;
                    retrieved_answer.Is_Correct = correct_answer_list[i] == j ? true : false;
                    
                    _context.Answer.Update(retrieved_answer);
                    _context.SaveChanges();
                    j++;
                };
                i++;
            }
            return true;

        }

        //Delete all quizzes created by a teach
        public async Task<bool> DeleteTeacherQuizzes(string Teacher_Id)
        {
            var quizzes_by_teacher = _context.Quiz.Where(q => q.User_Id == Teacher_Id).ToList();
            if (quizzes_by_teacher.Count == 0) return false;

            foreach(var quiz in quizzes_by_teacher)
            {
                var quiz_room_id = quiz.Quiz_Id;
                var quiz_room = await _context.QuizRoom.FindAsync(quiz_room_id);

                var questions = _context.Question.Where(q => q.Quiz_Id == quiz_room_id).ToList();
                if (questions.Count() == 0) return false;

                foreach (var question in questions)
                {
                    var answers = _context.Answer.Where(a => a.Question_Id == question.Id).ToList();
                    if (answers.Count() == 0) return false;
                    _context.Answer.RemoveRange(answers);
                    _context.SaveChanges();
                }

                //Remove Questions
                _context.Question.RemoveRange(questions);
                _context.SaveChanges();

                //Delete quiz history i.e if completed by any student
                var completed_quiz_by_users = _context.UserInRoom.Where(uir => uir.Quiz_Room_Id == quiz.Id).ToList();
                _context.UserInRoom.RemoveRange(completed_quiz_by_users);
                _context.SaveChanges();

                //Remove Quiz Room
                _context.QuizRoom.Remove(quiz_room);
                _context.SaveChanges();

                //Remove Quiz
                _context.Quiz.Remove(quiz);
                _context.SaveChanges();
            }

            
            return true;
        }

        //Delete all quizzes given by a student
        public bool DeleteStudentQuizzes(string Student_Id)
        {
            if (Student_Id == null) return false;
            var completed_quizzes = _context.UserInRoom.Where(uir => uir.User_Id == Student_Id);
            if (completed_quizzes.Count() == 0) return false;
            _context.UserInRoom.RemoveRange(completed_quizzes);
            _context.SaveChanges();
            return true;
        }

        public bool IsCompleted(string quiz_id)
        {
            var status = _context.UserInRoom.Any(uir => uir.Quiz_Room_Id == quiz_id);
            return status;
        }

    }
}
