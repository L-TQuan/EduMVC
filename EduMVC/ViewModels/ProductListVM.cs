using EduMVC.Enums;

namespace EduMVC.ViewModels
{
    public class ProductListVM
    {
        public IEnumerable<CourseVM>? Courses { get; set; }
        public bool IsTeacher { get; set; }
        public TeacherStatusEnum? TeacherStatus { get; set; }
    }
}
