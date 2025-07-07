namespace EduMVC.ViewModels
{
    public class LessonViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Position { get; set; }
        public IFormFile? MediumFile { get; set; }
        public string? MediumPath { get; set; }
        public List<IFormFile>? DocumentFiles { get; set; }
        public List<DocumentVM>? Documents { get; set; }

        // Foreign key to the Section
        public Guid SectionId { get; set; }
        public string SectionName { get; set; }
        public Guid CourseId { get; set; }
    }
}
