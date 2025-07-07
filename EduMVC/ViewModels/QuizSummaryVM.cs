namespace EduMVC.ViewModels
{
    public class QuizSummaryVM
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int Position { get; set; }
        public int? Questions { get; set; }
    }
}
