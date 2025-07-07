using EduMVC.Data;
using EduMVC.Helpers;
using EduMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduMVC.ViewComponents
{
    public class CourseDetailsList : ViewComponent
    {
        private readonly EduDbContext _context;
        public CourseDetailsList(EduDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(Guid courseId)
        {
            var course = await _context.Courses
                .Include(c => c.Sections.OrderBy(s => s.Position))
                    .ThenInclude(s => s.Lessons.OrderBy(l => l.Position))
                        .ThenInclude(l => l.Medium)
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Lessons)
                        .ThenInclude(l => l.Documents)
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Quizzes.OrderBy(q => q.Position))
                        .ThenInclude(q => q.Questions.OrderBy(qu => qu.Position))
                .SingleOrDefaultAsync(c => c.Id.Equals(courseId));

            var courseDetailsVM = new CourseDetailsVM
            {
                Sections = course?.Sections.Select(s => new SectionVM
                {
                    Id = s.Id,
                    Title = s.Title,
                    Lessons = s.Lessons.Select(l => new LessonVM
                    {
                        Id = l.Id,
                        Position = l.Position,
                        Name = l.Name,
                        MediumPath = FileHelper.GetMediaFilePath(l),
                        Documents = FileHelper.GetDocumentFilePaths(l)
                                        .Select(doc => new DocumentVM 
                                        { 
                                            DisplayName = doc.Name + Path.GetExtension(doc.FilePath),
                                            Path = doc.FilePath 
                                        })
                                        .ToList()
                    }).ToList(),
                    Quizzes = s.Quizzes.Select(q => new QuizVM
                    {
                        Id = q.Id,
                        Position = q.Position,
                        Name = q.Name,
                        Questions = q.Questions.Select(qu => new QuestionVM
                        {
                            Id = qu.Id,
                            Text = qu.Text,
                            CorrectAnswer = qu.CorrectAnswer
                        }).ToList()
                    }).ToList()
                }).ToList()
            };

            return View(courseDetailsVM);
        }
    }
}

