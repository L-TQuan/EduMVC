using System.Collections.ObjectModel;

namespace EduMVC.Data.Entities
{
    public class Section
    {
        public Section()
        {
            Id = Guid.NewGuid();
            Lessons = new Collection<Lesson>();
            Quizzes = new Collection<Quiz>();
        }
        public Guid Id { get; set; }
        public string Title { get; set; }
        public int Position { get; set; }

        // Foreign key to the parent Course
        public Guid CourseId { get; set; }
        public virtual Course Course { get; set; }

        // Navigation properties to Lessons and Quizzes
        public virtual ICollection<Lesson> Lessons { get; set; }
        public virtual ICollection<Quiz> Quizzes { get; set; }
    }
}
