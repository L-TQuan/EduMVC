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
    public class SectionController : Controller
    {
        private readonly EduDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public SectionController(EduDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Create(Guid courseId)
        {
            var course = await _context.Courses
                               .Where(c => c.Id.Equals(courseId))
                               .SingleOrDefaultAsync();

            if (course == null)
            {
                return NotFound();
            }

            var sectionViewModel = new SectionViewModel
            {
                CourseId = courseId,
                CourseName = course.Title,
                CourseDescription = course.Description,
            };
            return View(sectionViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SectionViewModel sectionVM)
        {
            try
            {
                var countSection = await _context.Sections
                                .Where(s => s.CourseId.Equals(sectionVM.CourseId))
                                .CountAsync();

                var section = new Section
                {
                    Title = sectionVM.Title,
                    CourseId = sectionVM.CourseId,
                    Position = countSection + 1,
                };

                _context.Sections.Add(section);
                await SetCourseToDraft(sectionVM.CourseId);
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(Create), new { courseId = sectionVM.CourseId });
            }
            catch
            {
                ViewBag.ErrorMessage = "An error occurred while creating the section.";
                return View(nameof(Create), sectionVM);
            }
        }

        public async Task<IActionResult> Edit(Guid sectionId)
        {
            var section = await _context.Sections
                    .Where(s => s.Id.Equals(sectionId))
                    .SingleOrDefaultAsync();
            if (section == null) { return BadRequest(); }

            var course = await _context.Courses
                               .Where(c => c.Id.Equals(section.CourseId))
                               .SingleOrDefaultAsync();

            var sectionVM = new SectionViewModel
            {
                Id = section.Id,
                Title = section.Title,
                CourseId = section.CourseId,
                CourseName = course.Title,
                CourseDescription = course.Description,
            };

            return View(nameof(Create), sectionVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SectionViewModel sectionVM)
        {
            try
            {
                var section = await _context.Sections.FindAsync(sectionVM.Id);

                if (section == null) { return BadRequest(); }
                //=== Fields will be changed ===//
                section.Title = sectionVM.Title;

                await SetCourseToDraft(sectionVM.CourseId);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Create), new { courseId = sectionVM.CourseId });
            }
            catch
            {
                ViewBag.ErrorMessage = "An error occurred while updating the course.";
                return View(nameof(Create), sectionVM);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid sectionId)
        {
            var status = false;
            var message = "Not executed";
            try
            {
                var section = await _context.Sections
                    .Include(s => s.Lessons)
                    .ThenInclude(l => l.Documents)
                    .Include(s => s.Lessons)
                    .ThenInclude(l => l.Medium)
                    .Where(s => s.Id.Equals(sectionId))
                    .SingleOrDefaultAsync();

                if (section != null)
                {
                    var webRoot = _environment.WebRootPath.Normalize();

                    // Loop through each lesson in the section and delete media/documents
                    foreach (var lesson in section.Lessons)
                    {
                        // Delete media file
                        var media = lesson.Medium;
                        if (media != null)
                        {
                            var mediaPath = Path.Combine(webRoot, "media/", media.FileName + "." + media.Extension);
                            if (System.IO.File.Exists(mediaPath))
                            {
                                System.IO.File.Delete(mediaPath);
                            }
                            _context.Media.Remove(media);
                        }

                        // Delete document files
                        foreach (var document in lesson.Documents)
                        {
                            var documentPath = Path.Combine(webRoot, "documents/", document.FileName + "." + document.Extension);
                            if (System.IO.File.Exists(documentPath))
                            {
                                System.IO.File.Delete(documentPath);
                            }
                            _context.Documents.Remove(document);
                        }
                    }

                    //=== Decrement Position ===//
                    var currentPosition = section.Position;
                    var listSection = await _context.Sections
                        .Where(s => s.Position > currentPosition)
                        .ToListAsync();

                    if (listSection != null && listSection.Count > 0)
                    {
                        foreach (var item in listSection)
                        {
                            item.Position -= 1;
                        }
                    }

                    await SetCourseToDraft(section.CourseId);
                    _context.Sections.Remove(section);
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


        public IActionResult ReloadSectionList(Guid courseId, int currentPage = 1)
        {
            return ViewComponent(nameof(SectionList), new { courseId, currentPage });
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
