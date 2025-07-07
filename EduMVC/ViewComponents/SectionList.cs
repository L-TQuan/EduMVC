using EduMVC.Common;
using EduMVC.Data;
using EduMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduMVC.ViewComponents
{
    public class SectionList : ViewComponent
    {
        private readonly EduDbContext _context;
        public SectionList(EduDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(Guid courseId, int currentPage = 1)
        {
            var take = Constants.TAKE;
            var skip = (currentPage - 1) * take;
            ViewBag.CurrentPage = currentPage;
            ViewBag.CourseId = courseId;

            var sectionList = await _context.Sections
                .Where(s => s.CourseId.Equals(courseId))
                .OrderBy(s => s.Position)
                .ToListAsync();
            if (sectionList != null && sectionList.Count() > 0)
            {
                var tempNumber = sectionList.Count / take;
                ViewBag.MaxPage = (sectionList.Count % take == 0) ? tempNumber : tempNumber + 1;
            }
            else
            {
                // Default to 1 page if no section
                ViewBag.MaxPage = 1;
            }

            var items = sectionList
                .Select(s => new SectionViewModel
                {
                    Id = s.Id,
                    Position = s.Position,
                    Title = s.Title,
                    CourseId = s.CourseId,
                })
                .Skip(skip)
                .Take(take)
                .ToList();
            return View(items);
        }
    }
}
