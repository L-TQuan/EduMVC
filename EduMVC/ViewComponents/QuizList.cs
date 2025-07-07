using EduMVC.Common;
using EduMVC.Data;
using EduMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduMVC.ViewComponents
{
    public class QuizList : ViewComponent
    {
        private readonly EduDbContext _context;
        public QuizList(EduDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(Guid sectionId, int currentPage = 1)
        {
            var take = Constants.TAKE;
            var skip = (currentPage - 1) * take;
            ViewBag.CurrentPage = currentPage;
            ViewBag.SectionId = sectionId;

            var quizList = await _context.Quizzes
                .Where(q => q.SectionId.Equals(sectionId))
                .OrderBy(q => q.Position)
                .ToListAsync();
            if (quizList != null && quizList.Count() > 0)
            {
                var tempNumber = quizList.Count / take;
                ViewBag.MaxPage = (quizList.Count % take == 0) ? tempNumber : tempNumber + 1;
            }
            else
            {
                // Default to 1 page if no quiz
                ViewBag.MaxPage = 1;
            }

            var items = quizList
                .Select(q => new QuizViewModel
                {
                    Id = q.Id,
                    Position = q.Position,
                    Name = q.Name,
                    SectionId = q.SectionId,
                })
                .Skip(skip)
                .Take(take)
                .ToList();
            return View(items);
        }
    }
}
