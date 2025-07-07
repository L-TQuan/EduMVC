namespace EduMVC.ViewModels
{
    public class SectionVM
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public int Position { get; set; }
        public IEnumerable<LessonVM> Lessons { get; set; }
        public IEnumerable<QuizVM> Quizzes { get; set; }
    }
}
