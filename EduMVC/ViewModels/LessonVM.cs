namespace EduMVC.ViewModels
{
    public class LessonVM
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Position { get; set; }
        public string? MediumPath { get; set; }
        public List<DocumentVM>? Documents { get; set; }
    }
}
