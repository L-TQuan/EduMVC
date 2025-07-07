using EduMVC.Enums;

namespace EduMVC.Areas.Identity.Data
{
    public class ProfileDocument
    {
        public ProfileDocument()
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

        // Foreign key to the user
        public string UserId { get; set; }
        public virtual EduUser User { get; set; }
    }
}
