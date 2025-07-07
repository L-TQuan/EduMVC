using EduMVC.Data;
using EduMVC.Data.Entities;
using EduMVC.Enums;
using EduMVC.ViewComponents;
using EduMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Collections.Specialized.BitVector32;

namespace EduMVC.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class QuestionController : Controller
    {
        private readonly EduDbContext _context;
        public QuestionController(EduDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Create(Guid quizId)
        {
            var quiz = await _context.Quizzes
                               .Where(q => q.Id.Equals(quizId))
                               .SingleOrDefaultAsync();

            if (quiz == null)
            {
                return NotFound();
            }

            var questionVM = new QuestionViewModel
            {
                SectionId = quiz.SectionId,
                QuizId = quiz.Id,
                QuizName = quiz.Name,
                QuizDescription = quiz.Description,
            };
            return View(questionVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuestionViewModel questionVM)
        {
            try
            {
                var countQuestion = await _context.Questions
                                .Where(qu => qu.QuizId.Equals(questionVM.QuizId))
                                .CountAsync();

                var question = new Question
                {
                    Text = questionVM.Text,
                    CorrectAnswer = questionVM.CorrectAnswer,
                    QuizId = questionVM.QuizId,
                    Position = countQuestion + 1,
                };

                _context.Questions.Add(question);

                var quiz = await _context.Quizzes
                    .FirstOrDefaultAsync(q => q.Id.Equals(question.QuizId));

                var section = await _context.Sections
                    .FirstOrDefaultAsync(s => s.Id.Equals(quiz.SectionId));

                await SetCourseToDraft(section.CourseId);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Create), new { quizId = questionVM.QuizId });
            }
            catch
            {
                ViewBag.ErrorMessage = "An error occurred while creating the lesson.";
                return View(nameof(Create), questionVM);
            }
        }

        public async Task<IActionResult> Edit(Guid questionId)
        {
            var question = await _context.Questions
                    .Where(qu => qu.Id.Equals(questionId))
                    .SingleOrDefaultAsync();
            if (question == null) { return BadRequest(); }

            var quiz = await _context.Quizzes
                               .Where(q => q.Id.Equals(question.QuizId))
                               .SingleOrDefaultAsync();

            var questionVM = new QuestionViewModel
            {
                Id = question.Id,
                Text = question.Text,
                CorrectAnswer = question.CorrectAnswer,
                QuizId = quiz.Id,
                QuizName = quiz.Name,
                QuizDescription = quiz.Description,
                SectionId = quiz.SectionId,
            };

            return View(nameof(Create), questionVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(QuestionViewModel questionVM)
        {
            try
            {
                var question = await _context.Questions
                    .Include(qu => qu.Quiz)
                        .ThenInclude(q => q.Section)
                    .Where(qu => qu.Id.Equals(questionVM.Id))
                    .FirstOrDefaultAsync();
                if (question == null) { return BadRequest(); }

                //=== Fields will be changed ===//
                question.Text = questionVM.Text;
                question.CorrectAnswer = questionVM.CorrectAnswer;

                await SetCourseToDraft(question.Quiz.Section.CourseId);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Create), new { quizId = questionVM.QuizId });
            }
            catch
            {
                ViewBag.ErrorMessage = "An error occurred while updating the course.";
                return View(nameof(Create), questionVM);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid questionId)
        {
            var status = false;
            var message = "Not executed";
            try
            {
                var question = await _context.Questions
                    .Include(qu => qu.Quiz)
                        .ThenInclude(q => q.Section)
                    .Where(qu => qu.Id.Equals(questionId))
                    .SingleOrDefaultAsync();
                if (question != null)
                {
                    //=== Step 1: Decreasement Position ===//
                    var currentPosition = question.Position;
                    var listQuestion = await _context.Questions
                        .Where(qu => qu.Position > currentPosition)
                        .ToListAsync();
                    if (listQuestion != null && listQuestion.Count > 0)
                    {
                        foreach (var item in listQuestion)
                        {
                            item.Position -= 1;
                        }
                    }

                    //=== Step 2: Remove quiz ===//
                    _context.Questions.Remove(question);
                    await SetCourseToDraft(question.Quiz.Section.CourseId);
                }

                await _context.SaveChangesAsync();
                status = true;
                message = "Success";
            }
            catch
            {
                message = "Error execution";
            }
            return Json(new { status, message });
        }

        public IActionResult ReloadQuestionList(Guid quizId, int currentPage, string? searchTerm = null)
        {
            return ViewComponent(nameof(QuestionList), new { quizId, currentPage, searchTerm });
        }

        private async Task SetCourseToDraft(Guid courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course != null)
            {
                course.PublishStatus = PublishStatus.Draft;
            }
        }
    }
}
