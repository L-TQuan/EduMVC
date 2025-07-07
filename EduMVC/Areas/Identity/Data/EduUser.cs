
using EduMVC.Enums;
using Microsoft.AspNetCore.Identity;

namespace EduMVC.Areas.Identity.Data
{
    public class EduUser : IdentityUser
    {
        public TeacherStatusEnum? Status { get; set; }
        public virtual ProfileDocument? ProfileDocument { get; set; }
    }
}
