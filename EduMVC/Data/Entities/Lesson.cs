using System.Collections.ObjectModel;

namespace EduMVC.Data.Entities
{
    public class Lesson
    {
        public Lesson()
        {
            Id = Guid.NewGuid();
            Documents = new Collection<Document>();
        }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Position { get; set; }

        //Lesson only has 1 media file
        public Guid? MediumId { get; set; }
        public virtual Medium Medium { get; set; }

        //Lesson has multiple documents
        public virtual ICollection<Document> Documents { get; set; }

        // Foreign key to the Section
        public Guid SectionId { get; set; }
        public virtual Section Section { get; set; }
    }
}
