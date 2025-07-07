namespace EduMVC.ViewModels
{
    public class CartItem
    {
        public Guid Id { get; set; }
        public CourseSummaryVM Product { get; set; }
        public decimal Price { get; set; }
    }
}
