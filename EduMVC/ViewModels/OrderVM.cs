namespace EduMVC.ViewModels
{
    public class OrderVM
    {
        // name of the user making the purchase
        public string UserName { get; set; }
        public DateTime OrderDate { get; set; }
        public int CourseAmount { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
