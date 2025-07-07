using EduMVC.Common;
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
    public class LessonController : BaseController
    {
        private readonly EduDbContext _context;
        private readonly IWebHostEnvironment _environment;
        public LessonController(EduDbContext context, IWebHostEnvironment environment)
            : base(context, environment)
        {
            _context = context;
            _environment = environment;
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

            var lessonVM = new LessonViewModel
            {
                SectionId = sectionId,
                SectionName = section.Title,
                CourseId = section.CourseId,
            };
            return View(lessonVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LessonViewModel lessonVM)
        {
            try
            {
                if (lessonVM.MediumFile?.Length > 100 * 1024 * 1024) // 100 MB limit
                {
                    ModelState.AddModelError("MediumFile", "Medium file exceed the 100 mb limit.");
                    return View(nameof(Create), lessonVM);
                }

                var media = await SaveMedia(lessonVM.MediumFile);
                var countLesson = await _context.Lessons
                                .Where(l => l.SectionId.Equals(lessonVM.SectionId))
                                .CountAsync();

                if (lessonVM.MediumFile == null && media == null)
                {
                    ModelState.AddModelError("MediumFile", "Invalid file type or no file was provided. A lesson needs a video file");
                    return View(nameof(Create), lessonVM);
                }

                var lesson = new Lesson
                {
                    Name = lessonVM.Name,
                    SectionId = lessonVM.SectionId,
                    MediumId = media != null ? media.Id : null,
                    Position = countLesson + 1,
                };

                if (lessonVM.DocumentFiles != null && lessonVM.DocumentFiles.Count > 0)
                {
                    foreach (var documentFile in lessonVM.DocumentFiles)
                    {
                        if (documentFile.Length > 5 * 1024 * 1024) // 5 MB limit
                        {
                            ModelState.AddModelError("DocumentFiles", "One or more document files exceed the 5 MB limit.");
                            return View(nameof(Create), lessonVM);
                        }

                        var document = await SaveDocument(lesson.Id, documentFile);

                        if ( document == null )
                        {
                            ModelState.AddModelError("DocumentFiles", "One or more document files failed to save. Please check the files and try again.");
                            return View(nameof(Create), lessonVM);
                        }

                        _context.Documents.Add(document);
                    }
                }

                _context.Lessons.Add(lesson);

                var section = await _context.Sections
                    //.Include(s => s.Course)
                    .FirstOrDefaultAsync(s => s.Id.Equals(lesson.SectionId));
                await SetCourseToDraft(section.CourseId);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Create), new { sectionId = lessonVM.SectionId });
            }
            catch
            {
                ViewBag.ErrorMessage = "An error occurred while creating the lesson.";
                return View(nameof(Create), lessonVM);
            }
        }

        public async Task<IActionResult> Edit(Guid lessonId)
        {
            var lesson = await _context.Lessons
                    .Include(l => l.Medium)
                    .Include(l => l.Documents)
                    .Where(l => l.Id.Equals(lessonId))
                    .SingleOrDefaultAsync();
            if (lesson == null) { return BadRequest(); }

            var section = await _context.Sections
                               .Where(s => s.Id.Equals(lesson.SectionId))
                               .SingleOrDefaultAsync();

            var lessonVM = new LessonViewModel
            {
                Id = lesson.Id,
                Name = lesson.Name,
                SectionId = section.Id,
                SectionName = section.Title,
                CourseId = section.CourseId,
            };

            return View(nameof(Create), lessonVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(LessonViewModel lessonVM)
        {
            try
            {
                var lesson = await _context.Lessons
                    .Include(l => l.Section)
                    .Where(l => l.Id.Equals(lessonVM.Id))
                    .FirstOrDefaultAsync();
                if (lesson == null) { return BadRequest(); }

                var webRoot = _environment.WebRootPath.Normalize();
                if (webRoot == null) { return BadRequest(); }

                //=== Fields will be changed ===//
                lesson.Name = lessonVM.Name;


                if (lessonVM.MediumFile != null)
                {
                    if (lessonVM.MediumFile.Length > 100 * 1024 * 1024) 
                    {
                        ModelState.AddModelError("MediumFile", "The medium file exceeds 100 MB limit.");
                        return View(nameof(Create), lessonVM);
                    }

                    // Delete the old media file if it exists
                    if (lesson.MediumId.HasValue)
                    {
                        var media = await _context.Media.FindAsync(lesson.MediumId.Value);
                        if (media != null)
                        {
                            var mediaPath = Path.Combine(webRoot, "media/", media.FileName + "." + media.Extension);
                            if (System.IO.File.Exists(mediaPath))
                            {
                                System.IO.File.Delete(mediaPath);
                            }

                            _context.Media.Remove(media);
                        }
                    }

                    // Save the new media file
                    var newMedia = await SaveMedia(lessonVM.MediumFile);
                    if (newMedia != null)
                    {
                        lesson.MediumId = newMedia.Id;
                    }
                    else
                    {
                        ModelState.AddModelError("MediumFile", "Invalid file type or error saving file");
                        return View(nameof(Create), lessonVM);
                    }
                }

                // Handle documents
                if (lessonVM.DocumentFiles != null && lessonVM.DocumentFiles.Count > 0)
                {
                    var documents = await _context.Documents
                        .Where(d => d.LessonId.Equals(lessonVM.Id))
                        .ToListAsync();

                    // Delete old documents
                    foreach (var document in documents)
                    {
                        var documentPath = Path.Combine(webRoot, "documents/", document.FileName + "." + document.Extension);
                        if (System.IO.File.Exists(documentPath))
                        {
                            System.IO.File.Delete(documentPath);
                        }
                        _context.Documents.Remove(document);
                    }

                    // Add new documents
                    foreach (var documentFile in lessonVM.DocumentFiles)
                    {
                        if (documentFile.Length > 5 * 1024 * 1024) 
                        {
                            ModelState.AddModelError("DocumentFiles", "One or more document files exceed the 5 MB limit.");
                            return View(nameof(Create), lessonVM);
                        }

                        var document = await SaveDocument(lesson.Id, documentFile);
                        if (document != null)
                        {
                            _context.Documents.Add(document);
                        }
                        else 
                        {
                            ModelState.AddModelError("DocumentFiles", "One or more document files with invalid type or failed to save.");
                            return View(nameof(Create), lessonVM);
                        }
                    }
                }

                await SetCourseToDraft(lesson.Section.CourseId);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Create), new { sectionId = lessonVM.SectionId });
            }
            catch
            {
                ViewBag.ErrorMessage = "An error occurred while updating the course.";
                return View(nameof(Create), lessonVM);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid lessonId)
        {
            var status = false;
            var message = "Not executed";
            try
            {
                var lesson = await _context.Lessons
                    .Include(l => l.Section)
                    .Where(l => l.Id.Equals(lessonId))
                    .SingleOrDefaultAsync();
                if (lesson != null)
                {
                    //=== Decreasement Position ===//
                    var currentPosition = lesson.Position;
                    var listLesson = await _context.Lessons
                        .Where(l => l.Position > currentPosition)
                        .ToListAsync();
                    if (listLesson != null && listLesson.Count > 0)
                    {
                        foreach (var item in listLesson)
                        {
                            item.Position -= 1;
                        }
                    }

                    // === Remove media and documents related to the lesson === //
                    var webRoot = _environment.WebRootPath.Normalize();

                    var medium = await _context.Media
                        .Where(m => m.Id.Equals(lesson.MediumId))
                        .SingleOrDefaultAsync();

                    if (medium != null)
                    {
                        // Delete media from the file system
                        var mediaPath = Path.Combine(webRoot, "media/", medium.FileName + "." + medium.Extension);
                        if (System.IO.File.Exists(mediaPath))
                        {
                            System.IO.File.Delete(mediaPath);
                        }
                        _context.Media.Remove(medium);
                    }

                    var documents = await _context.Documents
                        .Where(d => d.LessonId.Equals(lessonId))
                        .ToListAsync();

                    foreach (var document in documents)
                    {
                        // Delete documents from the file system
                        var documentPath = Path.Combine(webRoot, "documents/", document.FileName + "." + document.Extension);
                        if (System.IO.File.Exists(documentPath))
                        {
                            System.IO.File.Delete(documentPath);
                        }
                        _context.Documents.Remove(document);
                    }

                    //=== Remove lesson ===//
                    await SetCourseToDraft(lesson.Section.CourseId);
                    _context.Lessons.Remove(lesson);
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

        public IActionResult ReloadLessonList(Guid sectionId, int currentPage, string? searchTerm = null)
        {
            return ViewComponent(nameof(LessonList), new { sectionId, currentPage, searchTerm });
        }

        private async Task<Document?> SaveDocument(Guid lessonId, IFormFile documentFile)
        {
            var fileName = "";
            var fileGuidName = Guid.NewGuid().ToString();
            var fileExtension = "";
            if (documentFile == null || documentFile.Length <= 0) return null;
            var fileNameString = documentFile.FileName;
            if (string.IsNullOrEmpty(fileNameString))
            {
                return null;
            }
            try
            {
                string[] arrayExtension = fileNameString.Split('.');
                var fullFileName = "";
                if (arrayExtension != null && arrayExtension.Length > 0)
                {
                    for (int i = 0; i < arrayExtension.Length; i++)
                    {
                        var ext = arrayExtension[i];
                        if (Constants.INVALID_EXTENSION.Contains(ext))
                        {
                            return null;
                        }
                    }
                    fileName = arrayExtension[0];
                    fileExtension = arrayExtension[arrayExtension.Length - 1];
                    if (!Constants.VALID_DOCUMENT_EXTENSION.Contains(fileExtension) && !Constants.VALID_IMAGE_EXTENSION.Contains(fileExtension))
                    {
                        return null;
                    }
                    fullFileName = fileGuidName + "." + fileExtension;
                }
                var webRoot = _environment.WebRootPath.Normalize();
                var physicalDocumentPath = Path.Combine(webRoot, "documents/");
                if (!Directory.Exists(physicalDocumentPath))
                {
                    Directory.CreateDirectory(physicalDocumentPath);
                }
                var physicalPath = Path.Combine(physicalDocumentPath, fullFileName);
                using (var stream = System.IO.File.Create(physicalPath))
                {
                    await documentFile.CopyToAsync(stream);
                }

                //Create document
                var newDocument = new Document
                {
                    Name = fileName,
                    FileName = fileGuidName,
                    Extension = fileExtension,
                    LessonId = lessonId,
                    Type = FileTypeEnum.Document,
                };
                _context.Documents.Add(newDocument);
                return newDocument;
            }
            catch
            {

            }
            return null;
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
