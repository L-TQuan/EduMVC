using EduMVC.Enums;
using Microsoft.AspNetCore.Mvc;

namespace EduMVC.ViewModels
{
    [Bind("Id,Title,Description,OwnerId,Price,ImageFile,PreviewMediumFile")]
    public class CourseViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Position { get; set; }
        public PublishStatus PublishStatus { get; set; }

        public IFormFile? PreviewMediumFile { get; set; }
        public string? PreviewMediumPath { get; set; }

        public IFormFile? ImageFile { get; set; }
        public string? ImagePath { get; set; }

        // Foreign key to the user who created the course
        public string OwnerId { get; set; }
    }
}
