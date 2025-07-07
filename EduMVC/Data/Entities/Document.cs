using EduMVC.Enums;

namespace EduMVC.Data.Entities
{
    public class Document
    {
        public Document()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }

        //Original file name
        public string Name { get; set; }

        //File name stored in system for security
        public string FileName { get; set; }
        public string Extension { get; set; }
        public FileTypeEnum Type { get; set; }

        // Foreign key to the Lesson
        public Guid LessonId { get; set; }
        public virtual Lesson Lesson { get; set; }
    }
}
