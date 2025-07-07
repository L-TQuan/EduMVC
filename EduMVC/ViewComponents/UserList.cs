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
    public class UserList : ViewComponent
    {
        private readonly IdentityDbContext _identityContext;
        private readonly UserManager<EduUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly EduDbContext _context;
        public UserList(IdentityDbContext identityContext,
            UserManager<EduUser> userManager,
            RoleManager<IdentityRole> roleManager,
            EduDbContext context)
        {
            _identityContext = identityContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int currentPage = 1, string nameSearch = null, string roleFilter = null)
        {
            var take = Constants.TEST_TAKE;
            //var take = Constants.ADMIN_TAKE;
            var skip = (currentPage - 1) * take;
            ViewBag.CurrentPage = currentPage;

            var userList = await _identityContext.Users
                .Include(u => u.ProfileDocument)
                .ToListAsync();

            if (!string.IsNullOrEmpty(nameSearch))
            {
                userList = userList.Where(u => u.UserName.Contains(nameSearch, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var roles = _roleManager.Roles
                .Select(role => new RoleViewModel
                {
                    Id = role.Id,
                    Name = role.Name
                })
                .ToList();

            if (!string.IsNullOrEmpty(roleFilter))
            {
                userList = userList.Where(u => _userManager.GetRolesAsync(u).Result.Contains(roleFilter)).ToList();
            }

            if (userList != null && userList.Count() > 0)
            {
                var tempNumber = userList.Count / take;
                ViewBag.MaxPage = (userList.Count % take == 0) ? tempNumber : tempNumber + 1;
            }
            else
            {
                // Default to 1 page if no user
                ViewBag.MaxPage = 1;
            }

            // A dictionary mapping user IDs to role names
            var userRoles = new Dictionary<string, string>();
            foreach (var user in userList)
            {
                var roleName = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
                if (roleName != null)
                {
                    userRoles[user.Id] = roleName;
                }
            }

            var items = userList
                .OrderBy(u => u.UserName)
                .Select(u => new UserViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    RoleName = userRoles.ContainsKey(u.Id) ? userRoles[u.Id] : null,
                    Roles = roles,
                    Status = u.Status,
                    ProfileDocument = u.ProfileDocument != null
                        ? new ProfileDocumentViewModel
                        {
                            Id = u.ProfileDocument.Id,
                            DisplayName = u.ProfileDocument.Name,
                            Path = FileHelper.GetProfileDocumentFilePath(u)
                        }
                        : null
                })
                .Skip(skip)
                .Take(take)
                .ToList();

            return View(items);
        }
    }
}
