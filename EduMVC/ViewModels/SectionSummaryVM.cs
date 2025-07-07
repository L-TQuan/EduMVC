namespace EduMVC.ViewModels
{
    public class SectionSummaryVM
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public int Position { get; set; }
        public int? Lessons { get; set; }
        public IEnumerable<QuizSummaryVM>? Quizzes { get; set; }
    }
}
