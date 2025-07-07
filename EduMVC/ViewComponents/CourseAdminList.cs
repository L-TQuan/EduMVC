using EduMVC.Areas.Identity.Data;
using EduMVC.Common;
using EduMVC.Data;
using EduMVC.Enums;
using EduMVC.Helpers;
using EduMVC.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduMVC.ViewComponents
{
    public class CourseAdminList : ViewComponent
    {
        private readonly UserManager<EduUser> _userManager;
        private readonly EduDbContext _context;
        public CourseAdminList(UserManager<EduUser> userManager,
            EduDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(PublishStatus? status = null, string? searchTerm = null, int currentPage = 1)
        {
            var take = Constants.TEST_TAKE;
            //var take = Constants.ADMIN_TAKE;
            var skip = (currentPage - 1) * take;
            ViewBag.CurrentPage = currentPage;

            var courseQuery = _context.Courses
                .Include(c => c.Image)
                .Include(c => c.PreviewMedium)
                .Where(c => c.PublishStatus != PublishStatus.Draft);

            if (status.HasValue)
            {
                courseQuery = courseQuery.Where(c => c.PublishStatus == status.Value);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                courseQuery = courseQuery.Where(c => c.Title.Contains(searchTerm));
            }

            var courseList = await courseQuery.ToListAsync();

            if (courseList != null && courseList.Count() > 0)
            {
                var tempNumber = courseList.Count / take;
                ViewBag.MaxPage = (courseList.Count % take == 0) ? tempNumber : tempNumber + 1;
            }
            else
            {
                // Default to 1 page if no course
                ViewBag.MaxPage = 1;
            }

            // Get all owners in a single query
            var allUsers = await _userManager.Users.ToListAsync();
            var userDictionary = allUsers.ToDictionary(u => u.Id, u => u.UserName);

            var items = courseList
                .OrderBy(c => c.CreatedDate)
                .Select(c => new CourseAdminVM
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    Price = c.Price,
                    CreatedDate = c.CreatedDate,
                    PublishStatus = c.PublishStatus,
                    PreviewMediumPath = FileHelper.GetPreviewMediaFilePath(c),
                    ImagePath = FileHelper.GetImageFilePath(c),
                    OwnerName = userDictionary.ContainsKey(c.OwnerId) ? userDictionary[c.OwnerId] : "Unknown Owner",
                })
                .Skip(skip)
                .Take(take)
                .ToList();

            return View(items);
        }
    }
}
