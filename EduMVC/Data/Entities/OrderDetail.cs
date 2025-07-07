namespace EduMVC.Data.Entities
{
    public class OrderDetail
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid CourseId { get; set; }
        public decimal Price { get; set; }

        // Navigation property to the Order
        public Order Order { get; set; }

        // Navigation property to the Course
        public Course Course { get; set; }
    }
}
