namespace EduMVC.ViewModels
{
    public class CourseSummaryVM
    {
        public Guid Id { get; set; }
        public string? ImagePath { get; set; }
        public string Title { get; set; }
        public string OwnerName { get; set; }
        public decimal Price { get; set; }
    }
}
