using EduMVC.Common;
using EduMVC.Data;
using EduMVC.Helpers;
using EduMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduMVC.ViewComponents
{
    public class LessonList : ViewComponent
    {
        private readonly EduDbContext _context;
        public LessonList(EduDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(Guid sectionId, int currentPage = 1, string? searchTerm = null)
        {
            var take = Constants.TAKE;
            var skip = (currentPage - 1) * take;
            ViewBag.CurrentPage = currentPage;
            ViewBag.CourseId = sectionId;

            var lessonQuery = _context.Lessons
                .Where(l => l.SectionId.Equals(sectionId));

            // Apply search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                lessonQuery = lessonQuery.Where(l => l.Name.Contains(searchTerm));
            }

            var lessonList = await lessonQuery
                .Include(l => l.Medium)
                .Include(l => l.Documents)
                .OrderBy(l => l.Position)
                .ToListAsync();
            if (lessonList != null && lessonList.Count() > 0)
            {
                var tempNumber = lessonList.Count / take;
                ViewBag.MaxPage = (lessonList.Count % take == 0) ? tempNumber : tempNumber + 1;
            }
            else
            {
                // Default to 1 page if no lessons
                ViewBag.MaxPage = 1;
            }

            var items = lessonList
                .Select(l => new LessonViewModel
                {
                    Id = l.Id,
                    Position = l.Position,
                    Name = l.Name,
                    SectionId = l.SectionId,
                    MediumPath = FileHelper.GetMediaFilePath(l),
                    Documents = FileHelper.GetDocumentFilePaths(l)
                                        .Select(doc => new DocumentVM 
                                        { 
                                            DisplayName = doc.Name + Path.GetExtension(doc.FilePath), 
                                            Path = doc.FilePath 
                                        })
                                        .ToList()
                    
                })
                .Skip(skip)
                .Take(take)
                .ToList();
            return View(items);
        }
    }
}
