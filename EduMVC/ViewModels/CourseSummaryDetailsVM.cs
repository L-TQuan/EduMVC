using EduMVC.Enums;

namespace EduMVC.ViewModels
{
    public class CourseSummaryDetailsVM
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? ImagePath { get; set; }
        public string PreviewVideoPath { get; set; }
        public string OwnerName { get; set; }
        public decimal Price { get; set; }
        public CourseStatusEnum PublishStatus { get; set; }
        public DateOnly CreatedDate { get; set; }
        public IEnumerable<SectionSummaryVM>? Sections { get; set; }
        public double AverageRating { get; set; }
    }
}
