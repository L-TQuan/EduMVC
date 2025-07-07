using EduMVC.Areas.Identity.Data;
using EduMVC.Common;
using EduMVC.Data;
using EduMVC.Enums;
using EduMVC.Helpers;
using EduMVC.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace EduMVC.Areas.Identity.Pages.Account.Manage
{
    public class ProfileSubmissionModel : PageModel
    {
        private readonly IdentityDbContext _identityContext;
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<EduUser> _userManager;

        public ProfileSubmissionModel(
            IdentityDbContext identityContext,
            IWebHostEnvironment environment,
            UserManager<EduUser> userManager)
        {
            _identityContext = identityContext;
            _environment = environment;
            _userManager = userManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public ProfileDocumentInputModel Input { get; set; }

        public ProfileDocumentViewModel Document { get; set; }

        public class ProfileDocumentInputModel
        {
            [Display(Name = "Upload Profile Document")]
            public IFormFile ProfileDocumentFile { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Check if the user has a ProfileDocument
            var document = _identityContext.ProfileDocuments
                .FirstOrDefault(d => d.UserId == userId);

            if (document != null)
            {
                Document = new ProfileDocumentViewModel
                {
                    Id = document.Id,
                    DisplayName = document.Name,
                    Path = FileHelper.GetProfileDocumentFilePath(user)
                };
            }
            else
            {
                Document = null;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            if (Input.ProfileDocumentFile == null || Input.ProfileDocumentFile.Length == 0)
            {
                ModelState.AddModelError("", "Invalid file type or failed to save.");
                return Page();
            }

            var savedDocument = await SaveProfileDocument(userId, Input.ProfileDocumentFile);
            if (savedDocument == null)
            {
                ModelState.AddModelError("", "Invalid file type or failed to save.");
                return Page();
            }

            await _identityContext.SaveChangesAsync();
            StatusMessage = "Profile document uploaded successfully!";
            return RedirectToPage();
        }

        private async Task<ProfileDocument?> SaveProfileDocument(string userId, IFormFile documentFile)
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
                    if (!Constants.VALID_DOCUMENT_EXTENSION.Contains(fileExtension))
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
                //var physicalPath = Path.Combine(physicalDocumentPath, fullFileName);
                //using (var stream = System.IO.File.Create(physicalPath))
                //{
                //    await documentFile.CopyToAsync(stream);
                //}

                // Check if the user already has a document
                var existingDocument = await _identityContext.ProfileDocuments
                    .FirstOrDefaultAsync(d => d.UserId.Equals(userId));

                if (existingDocument != null)
                {
                    var oldFilePath = Path.Combine(webRoot, "documents/", existingDocument.FileName + "." + existingDocument.Extension);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }

                    // Update the existing document
                    existingDocument.Name = fileName;
                    existingDocument.FileName = fileGuidName;
                    existingDocument.Extension = fileExtension;
                    existingDocument.Type = FileTypeEnum.Document;

                    _identityContext.ProfileDocuments.Update(existingDocument);
                }
                else
                {
                    // Create a new document if none exists
                    existingDocument = new ProfileDocument
                    {
                        Name = fileName,
                        FileName = fileGuidName,
                        Extension = fileExtension,
                        UserId = userId,
                        Type = FileTypeEnum.Document,
                    };
                    _identityContext.ProfileDocuments.Add(existingDocument);
                }

                // Save the new file
                var physicalPath = Path.Combine(physicalDocumentPath, fullFileName);
                using (var stream = System.IO.File.Create(physicalPath))
                {
                    await documentFile.CopyToAsync(stream);
                }

                await _identityContext.SaveChangesAsync();
                return existingDocument;
            }
            catch
            {
            }
            return null;
        }
    }
}
