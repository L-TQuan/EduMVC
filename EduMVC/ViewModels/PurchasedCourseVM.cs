using EduMVC.Enums;

namespace EduMVC.ViewModels
{
    public class PurchasedCourseVM
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string OwnerName { get; set; }
        public string? ImagePath { get; set; }
        public string? PreviewVideoPath { get; set; }
        public PublishStatus PublishStatus { get; set; }
    }
}
