using EduMVC.Common;
using EduMVC.Data;
using EduMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduMVC.ViewComponents
{
    public class QuestionList : ViewComponent
    {
        private readonly EduDbContext _context;
        public QuestionList(EduDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(Guid quizId, int currentPage = 1, string? searchTerm = null)
        {
            var take = Constants.TAKE;
            var skip = (currentPage - 1) * take;
            ViewBag.CurrentPage = currentPage;
            ViewBag.QuizId = quizId;

            var questionQuery = _context.Questions
                .Where(qu => qu.QuizId.Equals(quizId));

            // Apply search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                questionQuery = questionQuery.Where(qu => qu.Text.Contains(searchTerm));
            }

            var questionList = await questionQuery
                .OrderBy(qu => qu.Position)
                .ToListAsync();
            if (questionList != null && questionList.Count() > 0)
            {
                var tempNumber = questionList.Count / take;
                ViewBag.MaxPage = (questionList.Count % take == 0) ? tempNumber : tempNumber + 1;
            }
            else
            {
                // Default to 1 page if no question
                ViewBag.MaxPage = 1;
            }

            var items = questionList
                .Select(qu => new QuestionViewModel
                {
                    Id = qu.Id,
                    Position = qu.Position,
                    Text = qu.Text,
                    CorrectAnswer = qu.CorrectAnswer,
                    QuizId = qu.QuizId,
                })
                .Skip(skip)
                .Take(take)
                .ToList();
            return View(items);
        }
    }
}
