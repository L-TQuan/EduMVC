namespace EduMVC.ViewModels
{
    public class SectionViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public int Position { get; set; }

        // Foreign key to the parent Course
        public Guid CourseId { get; set; }
        public string CourseName { get; set; }
        public string? CourseDescription { get; set; }
    }
}
