namespace EduMVC.ViewModels
{
    public class PurchasedCourseSurmmaryVM
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }

        // Average rating of the course
        public double AverageRating { get; set; }
        public UserRatingVM? UserRating { get; set; }
    }
}
