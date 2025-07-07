namespace EduMVC.Data.Entities
{
    public class Question
    {
        public Question()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }
        public string Text { get; set; }
        public string CorrectAnswer { get; set; }
        public int Position { get; set; }

        // Foreign key to the Quiz
        public Guid QuizId { get; set; }
        public virtual Quiz Quiz { get; set; }
    }
}
