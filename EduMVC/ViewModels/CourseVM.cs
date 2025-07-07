using EduMVC.Enums;

namespace EduMVC.ViewModels
{
    public class CourseVM
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string OwnerName { get; set; }
        public decimal Price { get; set; }
        public string? ImagePath { get; set; }
        public string PreviewVideoPath { get; set; }
        public DateOnly CreatedDate { get; set; }
        public CourseStatusEnum Status { get; set; }
        public bool IsTeacherPending { get; set; }
    }
}
