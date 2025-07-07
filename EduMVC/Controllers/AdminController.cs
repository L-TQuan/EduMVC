using EduMVC.Areas.Identity.Data;
using EduMVC.Data;
using EduMVC.Enums;
using EduMVC.ViewComponents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IdentityDbContext _identityContext;
        private readonly UserManager<EduUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly EduDbContext _context;
        private readonly IEmailSender _emailSender;
        public AdminController(IdentityDbContext identityContext,
            UserManager<EduUser> userManager,
            RoleManager<IdentityRole> roleManager,
            EduDbContext context,
            IEmailSender emailSender)
        {
            _identityContext = identityContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _emailSender = emailSender;
        }

        public IActionResult UserManage()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveUserChanges(string userId, string role, TeacherStatusEnum? status)
        {
            var user = await _identityContext.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (role != null && !currentRoles.Contains(role))
            {
                // Remove old role and add new role if changed
                foreach (var currentRole in currentRoles)
                {
                    await _userManager.RemoveFromRoleAsync(user, currentRole);
                }
                await _userManager.AddToRoleAsync(user, role);

                if (role == "Teacher")
                {
                    user.Status = status ?? TeacherStatusEnum.Pending;
                }
                else
                {
                    user.Status = null;
                }
            }
            else
            {
                user.Status = status;
            }

            await _identityContext.SaveChangesAsync();

            if (user.Status == TeacherStatusEnum.Approved)
            {
                var subject = "Your Teacher Profile Has Been Approved";
                var message = $@"
                    <p>Dear {user.UserName},</p>
                    <p>Congratulations! Your teacher profile has been approved. You can now create and manage courses on EduMVC.</p>
                    <p>Best regards,<br>EduMVC Team</p>";

                await _emailSender.SendEmailAsync(user.Email, subject, message);
            }

            return Json(new { success = true });
        }

        public IActionResult ReloadUserList(int currentPage = 1, string nameSearch = null, string roleFilter = null)
        {
            return ViewComponent(nameof(UserList), new { currentPage, nameSearch, roleFilter });
        }

        public IActionResult CourseApproval()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveCourseStatus(Guid courseId, PublishStatus status)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
            {
                return Json(new { success = false, message = "Course not found" });
            }

            // Update the status of the course
            course.PublishStatus = status;
            await _context.SaveChangesAsync();

            if (status == PublishStatus.Published)
            {
                var owner = await _userManager.FindByIdAsync(course.OwnerId);
                if (owner != null)
                {
                    var subject = "Your Course Has Been Published";
                    var message = $@"
                        <p>Dear {owner.UserName},</p>
                        <p>We are pleased to inform you that your course, <strong>{course.Title}</strong>, has been published and is now available for learners.</p>
                        <p>Thank you for contributing to EduMVC.</p>
                        <p>Best regards,<br>EduMVC Team</p>";

                    await _emailSender.SendEmailAsync(owner.Email, subject, message);
                }
            }

            return Json(new { success = true });
        }

        public IActionResult ReloadCourseList(int currentPage = 1, PublishStatus? status = null, string searchTerm = null)
        {
            return ViewComponent(nameof(CourseAdminList), new { currentPage, status, searchTerm });
        }
    }
}
