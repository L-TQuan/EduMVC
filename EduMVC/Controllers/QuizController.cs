using EduMVC.Data;
using EduMVC.Data.Entities;
using EduMVC.Enums;
using EduMVC.ViewComponents;
using EduMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduMVC.Controllers
{

    [Authorize(Roles = "Teacher")]
    public class QuizController : Controller
    {
        private readonly EduDbContext _context;
        public QuizController(EduDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Create(Guid sectionId)
        {
            var section = await _context.Sections
                               .Where(s => s.Id.Equals(sectionId))
                               .SingleOrDefaultAsync();

            if (section == null)
            {
                return NotFound();
            }

            var quizVM = new QuizViewModel
            {
                SectionId = sectionId,
                SectionName = section.Title,
                CourseId = section.CourseId,
            };
            return View(quizVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuizViewModel quizVM)
        {
            try
            {
                var countQuiz = await _context.Quizzes
                                .Where(q => q.SectionId.Equals(quizVM.SectionId))
                                .CountAsync();

                var quiz = new Quiz
                {
                    Name = quizVM.Name,
                    Description = quizVM.Description,
                    SectionId = quizVM.SectionId,
                    Position = countQuiz + 1,
                };

                _context.Quizzes.Add(quiz);
                var section = await _context.Sections
                    .FirstOrDefaultAsync(s => s.Id.Equals(quiz.SectionId));
                await SetCourseToDraft(section.CourseId);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Create), new { sectionId = quizVM.SectionId });
            }
            catch
            {
                ViewBag.ErrorMessage = "An error occurred while creating the lesson.";
                return View(nameof(Create), quizVM);
            }
        }

        public async Task<IActionResult> Edit(Guid quizId)
        {
            var quiz = await _context.Quizzes
                    .Where(q => q.Id.Equals(quizId))
                    .SingleOrDefaultAsync();
            if (quiz == null) { return BadRequest(); }

            var section = await _context.Sections
                               .Where(s => s.Id.Equals(quiz.SectionId))
                               .SingleOrDefaultAsync();

            var quizVM = new QuizViewModel
            {
                Id = quiz.Id,
                Name = quiz.Name,
                Description = quiz.Description,
                SectionId = section.Id,
                SectionName = section.Title,
                CourseId = section.CourseId,
            };

            return View(nameof(Create), quizVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(QuizViewModel quizVM)
        {
            try
            {
                var quiz = await _context.Quizzes
                    .Include(q => q.Section)
                    .Where(q => q.Id.Equals(quizVM.Id))
                    .FirstOrDefaultAsync();
                if (quiz == null) { return BadRequest(); }

                //=== Fields will be changed ===//
                quiz.Name = quizVM.Name;
                quiz.Description = quizVM.Description;

                await SetCourseToDraft(quiz.Section.CourseId);
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(Create), new { sectionId = quizVM.SectionId });
            }
            catch
            {
                ViewBag.ErrorMessage = "An error occurred while updating the course.";
                return View(nameof(Create), quizVM);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid quizId)
        {
            var status = false;
            var message = "Not executed";
            try
            {
                var quiz = await _context.Quizzes
                    .Include(q => q.Section)
                    .Where(q => q.Id.Equals(quizId))
                    .SingleOrDefaultAsync();
                if (quiz != null)
                {
                    //=== Step 1: Decreasement Position ===//
                    var currentPosition = quiz.Position;
                    var listQuiz = await _context.Quizzes
                        .Where(q => q.Position > currentPosition)
                        .ToListAsync();
                    if (listQuiz != null && listQuiz.Count > 0)
                    {
                        foreach (var item in listQuiz)
                        {
                            item.Position -= 1;
                        }
                    }

                    //=== Step 2: Remove quiz ===//
                    _context.Quizzes.Remove(quiz);
                }

                await SetCourseToDraft(quiz.Section.CourseId);
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

        public IActionResult ReloadQuizList(Guid sectionId, int currentPage)
        {
            return ViewComponent(nameof(QuizList), new { sectionId, currentPage });
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
