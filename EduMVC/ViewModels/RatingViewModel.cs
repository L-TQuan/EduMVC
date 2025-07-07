namespace EduMVC.ViewModels
{
    public class RatingViewModel
    {
        public Guid Id { get; set; }
        public string StudentName { get; set; }
        public int Stars { get; set; }
        public string Comment { get; set; }
        public DateOnly CreatedDate { get; set; }
    }
}
