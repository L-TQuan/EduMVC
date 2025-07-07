// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using EduMVC.Areas.Identity.Data;
using EduMVC.Data;
using EduMVC.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EduMVC.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<EduUser> _userManager;
        private readonly SignInManager<EduUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IdentityDbContext _identityContext;
        private readonly IWebHostEnvironment _environment;

        public IndexModel(
            UserManager<EduUser> userManager,
            SignInManager<EduUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IdentityDbContext identityContext,
            IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _identityContext = identityContext;
            _environment = environment;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public List<IdentityRole> Roles { get; set; }
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "User name")]
            public string Username { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Role")]
            public string Role { get; set; }
        }

        private async Task LoadAsync(EduUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var currentRole = roles.FirstOrDefault();

            Input = new InputModel
            {
                Username = userName,
                PhoneNumber = phoneNumber,
                Role = currentRole,
            };

            Roles = _roleManager.Roles
                .Where(r => r.Name != "Admin")
                .ToList();
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            bool isUpdated = false;

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (Input.Role != null && !currentRoles.Contains(Input.Role))
            {
                // Remove old role and add new role if changed

                foreach (var role in currentRoles)
                {
                    await _userManager.RemoveFromRoleAsync(user, role);
                }
                await _userManager.AddToRoleAsync(user, Input.Role);

                if (Input.Role == "Teacher")
                {
                    user.Status = TeacherStatusEnum.Pending;
                }
                else if (Input.Role == "Student")
                {
                    user.Status = null;

                    var webRoot = _environment.WebRootPath.Normalize();
                    // Check if the user already has a document
                    var existingDocument = await _identityContext.ProfileDocuments
                        .FirstOrDefaultAsync(d => d.UserId.Equals(user.Id));

                    if (existingDocument != null)
                    {
                        var FilePath = Path.Combine(webRoot, "documents/", existingDocument.FileName + "." + existingDocument.Extension);
                        if (System.IO.File.Exists(FilePath))
                        {
                            System.IO.File.Delete(FilePath);
                        }

                        _identityContext.ProfileDocuments.Remove(existingDocument);
                    }
                }
                await _identityContext.SaveChangesAsync();
                isUpdated = true;
            }

            var userName = await _userManager.GetUserNameAsync(user);
            if (Input.Username != userName)
            {
                var setUserNameResult = await _userManager.SetUserNameAsync(user, Input.Username);
                if (!setUserNameResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set user name.";
                    return RedirectToPage();
                }
                isUpdated = true;
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
                isUpdated = true;
            }

            await _signInManager.RefreshSignInAsync(user);
            if (isUpdated)
            {
                StatusMessage = "Your profile has been updated";
            }
            else
            {
                StatusMessage = "No changes were made to your profile";
            }
            return RedirectToPage();
        }
    }
}
