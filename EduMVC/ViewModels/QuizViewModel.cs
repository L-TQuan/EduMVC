namespace EduMVC.ViewModels
{
    public class QuizViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int Position { get; set; }
        public Guid SectionId { get; set; }
        public string SectionName { get; set; }
        public Guid CourseId { get; set; }
    }
}
