namespace EduMVC.ViewModels
{
    public class QuizVM
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int Position { get; set; }
        public IEnumerable<QuestionVM> Questions { get; set; }
    }
}
