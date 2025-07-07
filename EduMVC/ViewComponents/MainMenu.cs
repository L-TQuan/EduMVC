using EduMVC.Areas.Identity.Data;
using EduMVC.Enums;
using EduMVC.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduMVC.ViewComponents
{
    public class MainMenu : ViewComponent
    {
        private readonly UserManager<EduUser> _userManager;

        public MainMenu(UserManager<EduUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync(UserClaimsPrincipal);
            var model = new MainMenuViewModel
            {
                IsTeacherApproved = user?.Status == TeacherStatusEnum.Approved
            };
            return View(model);
        }
    }
}
