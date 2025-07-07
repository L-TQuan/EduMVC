using EduMVC.Areas.Identity.Data;
using EduMVC.Common;
using EduMVC.Data;
using EduMVC.Helpers;
using EduMVC.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduMVC.ViewComponents
{
    public class CourseList : ViewComponent
    {
        private readonly EduDbContext _context;
        private readonly UserManager<EduUser> _userManager;
        public CourseList(EduDbContext context, UserManager<EduUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(int currentPage = 1, string searchTerm = null)
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            if (currentUser == null)
            {
                // Return empty if no user is logged in
                return View(new List<CourseViewModel>());
            }

            var take = Constants.TAKE;
            var skip = (currentPage - 1) * take;
            ViewBag.CurrentPage = currentPage;

            var courseQuery = _context.Courses
                .Where(c => c.OwnerId.Equals(currentUser.Id));

            // Apply search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                courseQuery = courseQuery.Where(c => c.Title.Contains(searchTerm));
            }

            var courseList = await courseQuery
                .Include(c => c.Image)
                .Include(c => c.PreviewMedium)
                .ToListAsync();

            if (courseList != null && courseList.Count() > 0)
            {
                var tempNumber = courseList.Count / take;
                ViewBag.MaxPage = (courseList.Count % take == 0) ? tempNumber : tempNumber + 1;
            }
            else
            {
                // Default to 1 page if no courses
                ViewBag.MaxPage = 1;
            }

            var items = courseList
                .OrderBy(c => c.Position)
                .Select(c => new CourseViewModel
                {
                    Id = c.Id,
                    ImagePath = FileHelper.GetImageFilePath(c),
                    Title = c.Title,
                    PreviewMediumPath = FileHelper.GetPreviewMediaFilePath(c),
                    Price = c.Price,
                    Position = c.Position,
                    PublishStatus = c.PublishStatus,
                })
                .Skip(skip)
                .Take(take)
                .ToList();
            return View(items);
        }
    }
}
