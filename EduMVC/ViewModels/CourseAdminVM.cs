using EduMVC.Enums;

namespace EduMVC.ViewModels
{
    public class CourseAdminVM
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public DateOnly CreatedDate { get; set; }
        public PublishStatus PublishStatus { get; set; }
        public string? PreviewMediumPath { get; set; }
        public string? ImagePath { get; set; }
        public string OwnerName { get; set; }
    }
}
