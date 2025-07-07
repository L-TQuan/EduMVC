//using EduMVC.Areas.Identity.Data;
//using EduMVC.Common;
//using EduMVC.Data;
//using EduMVC.Enums;
//using EduMVC.Helpers;
//using EduMVC.ViewModels;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System.Security.Claims;

//namespace EduMVC.Controllers
//{
//    [Authorize(Roles = "Teacher")]
//    public class TeacherController : Controller
//    {
//        private readonly IdentityDbContext _identityContext;
//        private readonly IWebHostEnvironment _environment;
//        public TeacherController(IdentityDbContext identityContext, IWebHostEnvironment environment)
//        {
//            _identityContext = identityContext;
//            _environment = environment;
//        }

//        public IActionResult ProfileSubmission()
//        {
//            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the current user's ID
//            var user = _identityContext.Users
//                .Where(u => u.Id.Equals(userId))
//                .FirstOrDefault();

//            var document = _identityContext.ProfileDocuments
//                .Where(d => d.UserId.Equals(userId))
//                .FirstOrDefault();

//            ProfileDocumentViewModel model = null;
//            if (document != null)
//            {
//                model = new ProfileDocumentViewModel
//                {
//                    Id = document.Id,
//                    DisplayName = document.Name,
//                    Path = FileHelper.GetProfileDocumentFilePath(user)
//                };
//            }

//            return View(model);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> ProfileSubmission(IFormFile profileDocumentFile)
//        {
//            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//            if (userId == null) return RedirectToAction("Login", "Account");

//            if (profileDocumentFile == null || profileDocumentFile.Length == 0)
//            {
//                ModelState.AddModelError("", "Invalid file type or failed to save.");
//            }

//            // Save profile document
//            var savedDocument = await SaveProfileDocument(userId, profileDocumentFile);
//            if (savedDocument == null)
//            {
//                ModelState.AddModelError("", "Invalid file type or failed to save.");
//                return View();
//            }

//            await _identityContext.SaveChangesAsync();
//            return RedirectToAction(nameof(ProfileSubmission));
//        }

//        private async Task<ProfileDocument?> SaveProfileDocument(string userId, IFormFile documentFile)
//        {
//            var fileName = "";
//            var fileGuidName = Guid.NewGuid().ToString();
//            var fileExtension = "";
//            if (documentFile == null || documentFile.Length <= 0) return null;
//            var fileNameString = documentFile.FileName;
//            if (string.IsNullOrEmpty(fileNameString))
//            {
//                return null;
//            }
//            try
//            {
//                string[] arrayExtension = fileNameString.Split('.');
//                var fullFileName = "";
//                if (arrayExtension != null && arrayExtension.Length > 0)
//                {
//                    for (int i = 0; i < arrayExtension.Length; i++)
//                    {
//                        var ext = arrayExtension[i];
//                        if (Constants.INVALID_EXTENSION.Contains(ext))
//                        {
//                            return null;
//                        }
//                    }
//                    fileName = arrayExtension[0];
//                    fileExtension = arrayExtension[arrayExtension.Length - 1];
//                    if (!Constants.VALID_DOCUMENT_EXTENSION.Contains(fileExtension))
//                    {
//                        return null;
//                    }
//                    fullFileName = fileGuidName + "." + fileExtension;
//                }
//                var webRoot = _environment.WebRootPath.Normalize();
//                var physicalDocumentPath = Path.Combine(webRoot, "documents/");
//                if (!Directory.Exists(physicalDocumentPath))
//                {
//                    Directory.CreateDirectory(physicalDocumentPath);
//                }
//                var physicalPath = Path.Combine(physicalDocumentPath, fullFileName);
//                using (var stream = System.IO.File.Create(physicalPath))
//                {
//                    await documentFile.CopyToAsync(stream);
//                }

//                //Create document
//                var newDocument = new ProfileDocument
//                {
//                    Name = fileName,
//                    FileName = fileGuidName,
//                    Extension = fileExtension,
//                    UserId = userId,
//                    Type = FileTypeEnum.Document,
//                };
//                _identityContext.ProfileDocuments.Add(newDocument);
//                return newDocument;
//            }
//            catch
//            {
//            }
//            return null;
//        }
//    }
//}
