namespace EduMVC.ViewModels
{
    public class QuestionVM
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public string CorrectAnswer { get; set; }
        public int Position { get; set; }
    }
}
