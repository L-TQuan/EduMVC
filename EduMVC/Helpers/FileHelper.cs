using EduMVC.Areas.Identity.Data;
using EduMVC.Common;
using EduMVC.Data.Entities;

namespace EduMVC.Helpers
{
    public static class FileHelper
    {
        public static string GetImageFilePath(Course course)
        {
            return course.ImageId.HasValue
                            ? Constants.IMAGE_PATH + course.Image.FileName + "." + course.Image.Extension
                            : "";
        }

        public static string GetPreviewMediaFilePath(Course course)
        {
            return course.PreviewMediumId.HasValue
                            ? Constants.MEDIA_PATH + course.PreviewMedium.FileName + "." + course.PreviewMedium.Extension
                            : "";
        }

        public static string GetMediaFilePath(Lesson lesson)
        {
            return lesson.MediumId.HasValue
                            ? Constants.MEDIA_PATH + lesson.Medium.FileName + "." + lesson.Medium.Extension
                            : "";
        }

        public static List<string> GetDocumentFileNames(Lesson lesson)
        {
            var Names = new List<string>();
            foreach (var document in lesson.Documents)
            {
                Names.Add(document.Name + "." + document.Extension);
            }
            return Names;
        }

        public static List<(string Name, string FilePath)> GetDocumentFilePaths(Lesson lesson)
        {
            return lesson.Documents.Select(document =>
                (document.Name, Constants.DOCUMENT_PATH + document.FileName + "." + document.Extension)).ToList();
        }

        public static string GetProfileDocumentFilePath(EduUser user)
        {
            return user.ProfileDocument != null
                            ? Constants.DOCUMENT_PATH + user.ProfileDocument?.FileName + "." + user.ProfileDocument?.Extension
                            : "";
        }
    }
}
