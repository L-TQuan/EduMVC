using EduMVC.Areas.Identity.Data;
using EduMVC.Common;
using EduMVC.Data;
using EduMVC.Data.Entities;
using EduMVC.Enums;
using EduMVC.Helpers;
using EduMVC.ViewComponents;
using EduMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace EduMVC.Controllers
{
    public class CourseController : BaseController
    {
        private readonly UserManager<EduUser> _userManager;
        private readonly EduDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CourseController(UserManager<EduUser> userManager, EduDbContext context, IWebHostEnvironment environment)
            : base(context, environment)
        {
            _userManager = userManager;
            _context = context;
            _environment = environment;
        }

        [Authorize(Roles = "Teacher")]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Teacher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseViewModel courseViewModel)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    ViewBag.ErrorMessage = "User not found or not teacher role.";
                    return View(courseViewModel);
                }

                var userCourseCount = await _context.Courses
                                    .Where(c => c.OwnerId.Equals(currentUser.Id))
                                    .CountAsync();

                if (courseViewModel.ImageFile?.Length > 5 * 1024 * 1024) 
                {
                    ModelState.AddModelError("ImageFile", "Image file size must not exceed 5 MB.");
                    return View(nameof(Create), courseViewModel);
                }

                var image = await SaveImage(courseViewModel.ImageFile);

                if (courseViewModel.PreviewMediumFile?.Length > 10 * 1024 * 1024)
                {
                    ModelState.AddModelError("PreviewMediumFile", "Preview video file size must not exceed 5 MB.");
                    return View(nameof(Create), courseViewModel);
                }

                var previewMedia = await SavePreviewMedia(courseViewModel.PreviewMediumFile);
                if (courseViewModel.PreviewMediumFile == null && previewMedia == null)
                {
                    ModelState.AddModelError("PreviewMediumFile", "Invalid file type or no file was provided. A lesson needs a video file");
                    return View(nameof(Create), courseViewModel);
                }

                var course = new Course
                {
                    Title = courseViewModel.Title,
                    ImageId = image != null ? image.Id : null,
                    PreviewMediumId = previewMedia != null ? previewMedia.Id : null,
                    Price = courseViewModel.Price,
                    Description = courseViewModel.Description,
                    OwnerId = currentUser.Id,
                    Position = userCourseCount + 1,
                    PublishStatus = PublishStatus.Draft,
                    CreatedDate = DateOnly.FromDateTime(DateTime.Now)
                };

                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Create));
            }
            catch
            {
                ViewBag.ErrorMessage = "An error occurred while creating the course.";
                return View(nameof(Create), courseViewModel);
            }
        }

        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Edit(Guid courseId)
        {
            var course = await _context.Courses
                    .Include(c => c.Image)
                    .Include(c => c.PreviewMedium)
                    .Where(c => c.Id.Equals(courseId))
                    .SingleOrDefaultAsync();
            if (course == null) { return BadRequest(); }
            var courseVM = new CourseViewModel
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                Price = course.Price,
                PublishStatus = course.PublishStatus,
            };

            return View(nameof(Create), courseVM);
        }

        [Authorize(Roles = "Teacher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CourseViewModel courseVM)
        {
            try
            {
                var course = await _context.Courses.FindAsync(courseVM.Id);
                if (course == null) { return BadRequest(); }

                var webRoot = _environment.WebRootPath.Normalize();
                if (webRoot == null) { return BadRequest(); }

                //=== Fields will be changed ===//
                course.Title = courseVM.Title;
                course.Description = courseVM.Description;
                course.Price = courseVM.Price;
                course.CreatedDate = DateOnly.FromDateTime(DateTime.Now);

                if (course.PublishStatus == PublishStatus.Published)
                {
                    course.PublishStatus = PublishStatus.Draft;
                }

                if (courseVM.ImageFile != null)
                {
                    if (courseVM.ImageFile.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("ImageFile", "Image file size must not exceed 5 MB.");
                        return View(nameof(Create), courseVM);
                    }

                    // Delete the old media file if it exists
                    if (course.ImageId.HasValue)
                    {
                        var img = await _context.Images.FindAsync(course.ImageId.Value);
                        if (img != null)
                        {
                            var imgPath = Path.Combine(webRoot, "courseImages/", img.FileName + "." + img.Extension);
                            if (System.IO.File.Exists(imgPath))
                            {
                                System.IO.File.Delete(imgPath);
                            }

                            _context.Images.Remove(img);
                        }
                    }

                    // Save the new media file
                    var newImage = await SaveImage(courseVM.ImageFile);
                    if (newImage != null)
                    {
                        course.ImageId = newImage.Id;
                    }
                    else
                    {
                        ModelState.AddModelError("MediumFile", "Invalid file type or error saving file");
                        return View(nameof(Create), courseVM);
                    }
                }

                if (courseVM.PreviewMediumFile != null)
                {
                    if (courseVM.PreviewMediumFile.Length > 10 * 1024 * 1024)
                    {
                        ModelState.AddModelError("PreviewMediumFile", "Preview video file size must not exceed 10 MB.");
                        return View(nameof(Create), courseVM);
                    }

                    // Delete the old media file if it exists
                    if (course.PreviewMediumId.HasValue)
                    {
                        var media = await _context.PreviewMedia.FindAsync(course.PreviewMediumId.Value);
                        if (media != null)
                        {
                            var mediaPath = Path.Combine(webRoot, "media/", media.FileName + "." + media.Extension);
                            if (System.IO.File.Exists(mediaPath))
                            {
                                System.IO.File.Delete(mediaPath);
                            }

                            _context.PreviewMedia.Remove(media);
                        }
                    }

                    // Save the new media file
                    var newMedia = await SavePreviewMedia(courseVM.PreviewMediumFile);
                    if (newMedia != null)
                    {
                        course.PreviewMediumId = newMedia.Id;
                    }
                    else
                    {
                        ModelState.AddModelError("MediumFile", "Invalid file type or error saving file");
                        return View(nameof(Create), courseVM);
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Create));
            }
            catch
            {
                ViewBag.ErrorMessage = "An error occurred while updating the course.";
                return View(nameof(Create), courseVM);
            }
        }

        [Authorize(Roles = "Teacher")]
        [HttpPost]
        public async Task<IActionResult> Delete(Guid courseId)
        {
            var status = false;
            var message = "Not executed";
            var webRoot = _environment.WebRootPath.Normalize();
            try
            {
                var course = await _context.Courses
                    .Include(c => c.Sections)
                        .ThenInclude(s => s.Lessons)
                            .ThenInclude(l => l.Documents)
                    .Include(c => c.Sections)
                        .ThenInclude(s => s.Lessons)
                            .ThenInclude(l => l.Medium)
                    .Where(c => c.Id.Equals(courseId))
                    .SingleOrDefaultAsync();
                if (course != null)
                {
                    //=== Remove files in lessons related to the course ===//
                    // Check if the course has sections
                    if (course.Sections != null && course.Sections.Count > 0)
                    {
                        foreach (var section in course.Sections)
                        {
                            // Check if the section has lessons
                            if (section.Lessons != null && section.Lessons.Count > 0)
                            {
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
                                    if (lesson.Documents != null && lesson.Documents.Count > 0)
                                    {
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
                                }
                            }
                        }
                    }

                    //Decreasement Position ===//
                    var currentPosition = course.Position;
                    var listCourse = await _context.Courses
                        .Where(c => c.Position > currentPosition)
                        .ToListAsync();
                    if (listCourse != null && listCourse.Count > 0)
                    {
                        foreach (var item in listCourse)
                        {
                            item.Position -= 1;
                        }
                    }

                    // === Remove preview video and image related to the course === //
                    var previewMedium = await _context.PreviewMedia
                        .Where(pm => pm.Id.Equals(course.PreviewMediumId))
                        .SingleOrDefaultAsync();

                    if (previewMedium != null)
                    {
                        // Delete media from the file system
                        var mediaPath = Path.Combine(webRoot, "media/", previewMedium.FileName + "." + previewMedium.Extension);
                        if (System.IO.File.Exists(mediaPath))
                        {
                            System.IO.File.Delete(mediaPath);
                        }
                        _context.PreviewMedia.Remove(previewMedium);
                    }

                    var img = await _context.Images
                        .Where(i => i.Id.Equals(course.ImageId))
                        .SingleOrDefaultAsync();

                    if (img != null)
                    {
                        // Delete media from the file system
                        var imgPath = Path.Combine(webRoot, "courseImages/", img.FileName + "." + img.Extension);
                        if (System.IO.File.Exists(imgPath))
                        {
                            System.IO.File.Delete(imgPath);
                        }
                        _context.Images.Remove(img);
                    }

                    //Remove course ===//
                    _context.Courses.Remove(course);
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

        [Authorize(Roles = "Teacher")]
        [HttpPost]
        public async Task<IActionResult> PublishCourse(Guid id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            // Change the status to Pending
            course.PublishStatus = PublishStatus.Pending;
            await _context.SaveChangesAsync();

            return Ok();
        }

        public async Task<IActionResult> ProductList(CourseSearchVM searchModel)
        {
            //=== Get data Shopping Cart ===//
            var cart = HttpContext.Session.GetString(Constants.SESSION_CART);
            List<Guid> cartCourses = new List<Guid>();

            if (cart != null)
            {
                var shoppingCart = JsonConvert.DeserializeObject<ShoppingCart>(cart);
                ViewBag.ShoppingCart = shoppingCart;

                // Extract course IDs
                cartCourses = shoppingCart?.CourseIds?.ToList() ?? new List<Guid>();
            }

            var currentUserId = _userManager.GetUserId(User);
            var isTeacher = User.IsInRole("Teacher");

            TeacherStatusEnum? teacherStatus = null;
            if (isTeacher)
            {
                var user = await _userManager.GetUserAsync(User);
                teacherStatus = user?.Status;
            }

            // Get all owners in a single query
            var allUsers = await _userManager.Users.ToListAsync();
            var userDictionary = allUsers.ToDictionary(u => u.Id, u => u.UserName);

            // Retrieve the orders for the user
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Course)
                .Where(o => o.UserId.Equals(currentUserId))
                .ToListAsync();

            var purchasedCourses = orders
                .SelectMany(o => o.OrderDetails.Select(od => od.CourseId))
                .ToList();


            var query = _context.Courses
                .Include(c => c.Image)
                .Include(c => c.PreviewMedium)
                .Where(c => c.PublishStatus == PublishStatus.Published);

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchModel.SearchName))
            {
                query = query.Where(c =>c.Title.Contains(searchModel.SearchName));
            }

            if (searchModel.MinPrice.HasValue)
            {
                query = query.Where(c => c.Price >= searchModel.MinPrice.Value);
            }

            if (searchModel.MaxPrice.HasValue)
            {
                query = query.Where(c => c.Price <= searchModel.MaxPrice.Value);
            }

            var courses = await query
                .Select(c => new CourseVM
                {
                    Id = c.Id,
                    Title = c.Title,
                    Price = c.Price,
                    // Use the dictionary to get Owner's name
                    OwnerName = userDictionary.ContainsKey(c.OwnerId) ? userDictionary[c.OwnerId] : "Unknown Owner",
                    ImagePath = FileHelper.GetImageFilePath(c),
                    PreviewVideoPath = FileHelper.GetPreviewMediaFilePath(c),
                    CreatedDate = c.CreatedDate,
                    Status = c.OwnerId.Equals(currentUserId) ? CourseStatusEnum.Owned :
                                purchasedCourses.Contains(c.Id) ? CourseStatusEnum.Purchased :
                                cartCourses.Contains(c.Id) ? CourseStatusEnum.InCart :
                                CourseStatusEnum.Available,
                })
                .ToArrayAsync();

            var model = new ProductListVM
            {
                Courses = courses,
                IsTeacher = isTeacher,
                TeacherStatus = teacherStatus
            };
            return View(model);
        }

        public async Task<IActionResult> SummaryDetails(Guid courseId)
        {
            //=== Get data Shopping Cart ===//
            var cart = HttpContext.Session.GetString(Constants.SESSION_CART);
            List<Guid> cartCourses = new List<Guid>();

            if (cart != null)
            {
                var shoppingCart = JsonConvert.DeserializeObject<ShoppingCart>(cart);
                ViewBag.ShoppingCart = shoppingCart;

                // Extract course IDs
                cartCourses = shoppingCart?.CourseIds?.ToList() ?? new List<Guid>();
            }

            var currentUserId = _userManager.GetUserId(User);

            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Course)
                .Where(o => o.UserId.Equals(currentUserId))
                .ToListAsync();
            var purchasedCourses = orders
                .SelectMany(o => o.OrderDetails.Select(od => od.CourseId))
                .ToList();

            var course = await _context.Courses
                .Where(c => c.Id.Equals(courseId))
                .Include(c => c.Image)
                .Include(c => c.PreviewMedium)
                .Include(c => c.Ratings)
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Lessons) 
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Quizzes) 
                        .ThenInclude(q => q.Questions)
                .FirstOrDefaultAsync();

            if (course == null)
            {
                return NotFound("Course not found.");
            }


            var owner = await _userManager.FindByIdAsync(course.OwnerId);
            var ownerName = owner?.UserName ?? "Unknown";

            // Calculate the average rating
            var ratings = course.Ratings;
            var averageRating = ratings.Any() ? ratings.Average(r => r.Stars) : 0;

            // Create the ViewModel
            var viewModel = new CourseSummaryDetailsVM
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                ImagePath = FileHelper.GetImageFilePath(course),
                PreviewVideoPath = FileHelper.GetPreviewMediaFilePath(course),
                OwnerName = ownerName,
                Price = course.Price,
                CreatedDate = course.CreatedDate,
                PublishStatus = course.OwnerId.Equals(currentUserId) ? CourseStatusEnum.Owned :
                                purchasedCourses.Contains(course.Id) ? CourseStatusEnum.Purchased :
                                cartCourses.Contains(course.Id) ? CourseStatusEnum.InCart :
                                CourseStatusEnum.Available,
                AverageRating = averageRating,
                Sections = course.Sections
                .OrderBy(s => s.Position)
                .Select(s => new SectionSummaryVM
                {
                    Id = s.Id,
                    Title = s.Title,
                    Position = s.Position,
                    Lessons = s.Lessons.Count(),
                    Quizzes = s.Quizzes
                    .OrderBy(q => q.Position)
                    .Select(q => new QuizSummaryVM
                    {
                        Id = q.Id,
                        Name = q.Name,
                        Description = q.Description,
                        Position = q.Position,
                        Questions = q.Questions.Count()
                    }).ToArray()
                }).ToArray()
            };

            return View(viewModel);
        }


        [Authorize]
        public async Task<IActionResult> MyLearning(string? searchName = null)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null)
            {
                return NotFound(); // Or redirect to an appropriate page
            }

            // Retrieve the orders for the user
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Course)
                        .ThenInclude(c => c.Image)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Course)
                        .ThenInclude(c => c.PreviewMedium)
                .Where(o => o.UserId.Equals(currentUserId))
                .ToListAsync();

            // Flatten the list of courses from order details
            var purchasedCourses = new List<PurchasedCourseVM>();

            foreach (var order in orders)
            {
                foreach (var orderDetail in order.OrderDetails)
                {
                    var course = orderDetail.Course;
                    if (course != null)
                    {
                        var owner = await _userManager.FindByIdAsync(course.OwnerId);

                        purchasedCourses.Add(new PurchasedCourseVM
                        {
                            Id = course.Id,
                            Title = course.Title,
                            OwnerName = owner.UserName,
                            ImagePath = FileHelper.GetImageFilePath(course),
                            PreviewVideoPath = FileHelper.GetPreviewMediaFilePath(course),
                            PublishStatus = course.PublishStatus,
                        });
                    }
                }
            }

            if (!string.IsNullOrEmpty(searchName))
            {
                purchasedCourses = purchasedCourses
                    .Where(c => c.Title.Contains(searchName, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return View(purchasedCourses);
        }

        [Authorize]
        public async Task<IActionResult> Details(Guid courseId)
        {
            var course = await _context.Courses
                .Include(c => c.Ratings)
                .SingleOrDefaultAsync(c => c.Id.Equals(courseId));

            if (course == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            var ratings = course.Ratings;
            var averageRating = ratings.Any() ? ratings.Average(r => r.Stars) : 0;
            var userRating = ratings.FirstOrDefault(r => r.StudentId.Equals(currentUserId));

            var courseVM = new PurchasedCourseSurmmaryVM
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                AverageRating = averageRating,
                UserRating = new UserRatingVM
                {
                    UserRatingStars = userRating?.Stars ?? 0,
                    UserComment = userRating?.Comment
                }
            };

            return View(courseVM);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SubmitRating(Guid courseId, int stars, string comment)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            var existingRating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.CourseId.Equals(courseId) && r.StudentId.Equals(userId));

            if (existingRating != null)
            {
                // Update existing rating
                existingRating.Stars = stars;
                existingRating.Comment = comment;
                existingRating.CreatedDate = DateOnly.FromDateTime(DateTime.Now);
            }
            else
            {
                // Add new rating
                var newRating = new Rating
                {
                    CourseId = courseId,
                    StudentId = userId,
                    Stars = stars,
                    Comment = comment,
                    CreatedDate = DateOnly.FromDateTime(DateTime.Now)
                };
                _context.Ratings.Add(newRating);
            }

            await _context.SaveChangesAsync();
            var updatedAverageRating = await _context.Ratings
                .Where(r => r.CourseId.Equals(courseId))
                .AverageAsync(r => r.Stars);

            return Json(new { success = true, averageRating = updatedAverageRating });
        }

        public IActionResult ReloadCourseList(int currentPage = 1, string searchTerm = null)
        {
            return ViewComponent(nameof(CourseList), new { currentPage, searchTerm });
        }

        public IActionResult ReloadRatingList(Guid courseId, int currentPage = 1)
        {
            return ViewComponent(nameof(RatingList), new { courseId, currentPage });
        }
    }
}
