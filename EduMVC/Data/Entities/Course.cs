using EduMVC.Enums;
using System.Collections.ObjectModel;

namespace EduMVC.Data.Entities
{
    public class Course
    {
        public Course()
        {
            Id = Guid.NewGuid();
            Sections = new Collection<Section>();
            Ratings = new Collection<Rating>();
        }
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public int Position { get; set; }
        public decimal Price { get; set; }
        //public decimal? Discount { get; set; }
        public PublishStatus PublishStatus { get; set; }
        public DateOnly CreatedDate { get; set; }

        // A course can have 1 image
        public Guid? ImageId { get; set; }
        public virtual Image Image { get; set; }

        // A course has 1 preview video
        public Guid? PreviewMediumId { get; set; }
        public virtual PreviewMedium PreviewMedium { get; set; }
        public string OwnerId { get; set; }
        public virtual ICollection<Section> Sections { get; set; }
        public virtual ICollection<Rating> Ratings { get; set; }
    }
}
