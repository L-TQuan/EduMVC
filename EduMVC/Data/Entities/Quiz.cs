namespace EduMVC.Data.Entities
{
    public class Quiz
    {
        public Quiz()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int Position { get; set; }

        // Foreign key to the Section
        public Guid SectionId { get; set; }
        public virtual Section Section { get; set; }

        //A quiz can have many questions
        public virtual ICollection<Question> Questions { get; set; }
    }
}
