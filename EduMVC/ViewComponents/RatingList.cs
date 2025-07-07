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
    public class RatingList : ViewComponent
    {
        private readonly EduDbContext _context;
        private readonly UserManager<EduUser> _userManager;

        public RatingList(EduDbContext context,
            UserManager<EduUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(Guid courseId, int currentPage = 1)
        {
            var take = Constants.TEST_TAKE;
            //var take = Constants.ADMIN_TAKE;
            var skip = (currentPage - 1) * take;
            ViewBag.CurrentPage = currentPage;

            var ratingList = await _context.Ratings
                .Where(r => r.CourseId.Equals(courseId))
                .ToListAsync();

            if (ratingList != null && ratingList.Count() > 0)
            {
                var tempNumber = ratingList.Count / take;
                ViewBag.MaxPage = (ratingList.Count % take == 0) ? tempNumber : tempNumber + 1;
            }
            else
            {
                // Default to 1 page if no rating
                ViewBag.MaxPage = 1;
            }

            var allUsers = await _userManager.Users.ToListAsync();
            var userDictionary = allUsers.ToDictionary(u => u.Id, u => u.UserName);

            var items = ratingList
                .Select(r => new RatingViewModel
                {
                    Id = r.Id,
                    StudentName = userDictionary.ContainsKey(r.StudentId) ? userDictionary[r.StudentId] : "Unknown Student",
                    Stars = r.Stars,
                    Comment = r.Comment,
                    CreatedDate = r.CreatedDate
                })
                .Skip(skip)
                .Take(take)
                .ToList();
            return View(items);
        }
    }
}
