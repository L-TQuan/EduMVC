namespace EduMVC.ViewModels
{
    public class QuestionViewModel
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public string CorrectAnswer { get; set; }
        public int Position { get; set; }
        public Guid QuizId { get; set; }
        public string QuizName { get; set; }
        public string QuizDescription { get; set; }
        public Guid SectionId { get; set; }
    }
}
