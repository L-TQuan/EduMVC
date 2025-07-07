namespace EduMVC.Data.Entities
{
    public class Rating
    {
        public Rating() 
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }
        public string StudentId { get; set; } 
        public int Stars { get; set; }     
        public string Comment { get; set; } 
        public DateOnly CreatedDate { get; set; } 

        public Guid CourseId { get; set; }
        public Course Course { get; set; }
    }
}
