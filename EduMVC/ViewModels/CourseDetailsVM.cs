namespace EduMVC.ViewModels
{
    public class CourseDetailsVM
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public IEnumerable<SectionVM>? Sections { get; set; }
    }
}
