using EduMVC.Enums;

namespace EduMVC.ViewModels
{
    public class UserViewModel
    {
        public UserViewModel()
        {
            // Initialize to avoid null references
            Roles = new List<RoleViewModel>();
        }

        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string RoleName { get; set; }

        // List of roles to choose
        public IEnumerable<RoleViewModel> Roles { get; set; }
        public TeacherStatusEnum? Status { get; set; }
        public virtual ProfileDocumentViewModel? ProfileDocument { get; set; }
    }
}
