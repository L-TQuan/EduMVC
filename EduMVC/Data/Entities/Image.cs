using EduMVC.Enums;

namespace EduMVC.Data.Entities
{
    public class Image
    {
        public Image()
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
        public virtual Course Course { get; set; }
    }
}
