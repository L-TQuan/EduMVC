namespace EduMVC.Common
{
    public static class Constants
    {
        public readonly static string[] INVALID_EXTENSION = { "bat", "com", "exe" };
        public readonly static string[] VALID_VIDEO_EXTENSION = { "mp3", "ogg", "mp4" };
        public readonly static string[] VALID_IMAGE_EXTENSION = { "png", "jpg", "gif", "jfif", "jpeg", "svg" };
        public readonly static string[] VALID_DOCUMENT_EXTENSION = { "pptx", "xlsx", "doc", "docx", "doc", "pdf" };
        public const string MEDIA_PATH = "~/media/";
        public const string IMAGE_PATH = "~/courseImages/";
        public const string DOCUMENT_PATH = "~/documents/";
        public const string SQL_SERVER = "LAPTOP-1D73JAA8\\SQLEXPRESS";

        public const int TAKE = 5;
        public const int ADMIN_TAKE = 10;
        public const int TEST_TAKE = 2;
        public const string SESSION_CART = "ShoppingCart";
    }
}
