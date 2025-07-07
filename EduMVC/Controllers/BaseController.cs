using EduMVC.Common;
using EduMVC.Data;
using EduMVC.Data.Entities;
using EduMVC.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduMVC.Controllers
{
    public class BaseController : Controller
    {
        private readonly EduDbContext _context;
        private readonly IWebHostEnvironment _environment;
        public BaseController(EduDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }
        protected async Task<Medium?> SaveMedia(IFormFile file)
        {
            var fileName = "";
            var fileGuidName = Guid.NewGuid().ToString();
            var fileExtension = "";
            if (file == null || file.Length <= 0) return null;
            var fileNameString = file.FileName;
            if (string.IsNullOrEmpty(fileNameString))
            {
                return null;
            }
            try
            {
                string[] arrayExtension = fileNameString.Split('.');
                var fullFileName = "";
                if (arrayExtension != null && arrayExtension.Length > 0)
                {
                    for (int i = 0; i < arrayExtension.Length; i++)
                    {
                        var ext = arrayExtension[i];
                        if (Constants.INVALID_EXTENSION.Contains(ext))
                        {
                            return null;
                        }
                    }
                    fileName = arrayExtension[0];
                    fileExtension = arrayExtension[arrayExtension.Length - 1];
                    if (!Constants.VALID_VIDEO_EXTENSION.Contains(fileExtension))
                    {
                        return null;
                    }
                    fullFileName = fileGuidName + "." + fileExtension;
                }
                var webRoot = _environment.WebRootPath.Normalize();
                var physicalVideoPath = Path.Combine(webRoot, "media/");
                if (!Directory.Exists(physicalVideoPath))
                {
                    Directory.CreateDirectory(physicalVideoPath);
                }
                var physicalPath = Path.Combine(physicalVideoPath, fullFileName);
                using (var stream = System.IO.File.Create(physicalPath))
                {
                    await file.CopyToAsync(stream);
                }

                //Create media
                var newMedium = new Medium
                {
                    Name = fileName,
                    FileName = fileGuidName,
                    Extension = fileExtension,
                    Type = FileTypeEnum.Video,
                };
                _context.Media.Add(newMedium);
                return newMedium;
            }
            catch
            {
            }
            return null;
        }

        protected async Task<Image?> SaveImage(IFormFile file)
        {
            var fileName = "";
            var fileGuidName = Guid.NewGuid().ToString();
            var fileExtension = "";
            if (file == null || file.Length <= 0) return null;
            var fileNameString = file.FileName;
            if (string.IsNullOrEmpty(fileNameString))
            {
                return null;
            }
            try
            {
                string[] arrayExtension = fileNameString.Split('.');
                var fullFileName = "";
                if (arrayExtension != null && arrayExtension.Length > 0)
                {
                    for (int i = 0; i < arrayExtension.Length; i++)
                    {
                        var ext = arrayExtension[i];
                        if (Constants.INVALID_EXTENSION.Contains(ext))
                        {
                            return null;
                        }
                    }
                    fileName = arrayExtension[0];
                    fileExtension = arrayExtension[arrayExtension.Length - 1];
                    if (!Constants.VALID_IMAGE_EXTENSION.Contains(fileExtension))
                    {
                        return null;
                    }
                    fullFileName = fileGuidName + "." + fileExtension;
                }
                var webRoot = _environment.WebRootPath.Normalize();
                var physicalVideoPath = Path.Combine(webRoot, "courseImages/");
                if (!Directory.Exists(physicalVideoPath))
                {
                    Directory.CreateDirectory(physicalVideoPath);
                }
                var physicalPath = Path.Combine(physicalVideoPath, fullFileName);
                using (var stream = System.IO.File.Create(physicalPath))
                {
                    await file.CopyToAsync(stream);
                }

                //Create media
                var newImage = new Image
                {
                    Name = fileName,
                    FileName = fileGuidName,
                    Extension = fileExtension,
                    Type = FileTypeEnum.Image,
                };
                _context.Images.Add(newImage);
                return newImage;
            }
            catch
            {
            }
            return null;
        }


        protected async Task<PreviewMedium?> SavePreviewMedia(IFormFile file)
        {
            var fileName = "";
            var fileGuidName = Guid.NewGuid().ToString();
            var fileExtension = "";
            if (file == null || file.Length <= 0) return null;
            var fileNameString = file.FileName;
            if (string.IsNullOrEmpty(fileNameString))
            {
                return null;
            }
            try
            {
                string[] arrayExtension = fileNameString.Split('.');
                var fullFileName = "";
                if (arrayExtension != null && arrayExtension.Length > 0)
                {
                    for (int i = 0; i < arrayExtension.Length; i++)
                    {
                        var ext = arrayExtension[i];
                        if (Constants.INVALID_EXTENSION.Contains(ext))
                        {
                            return null;
                        }
                    }
                    fileName = arrayExtension[0];
                    fileExtension = arrayExtension[arrayExtension.Length - 1];
                    if (!Constants.VALID_VIDEO_EXTENSION.Contains(fileExtension))
                    {
                        return null;
                    }
                    fullFileName = fileGuidName + "." + fileExtension;
                }
                var webRoot = _environment.WebRootPath.Normalize();
                var physicalVideoPath = Path.Combine(webRoot, "media/");
                if (!Directory.Exists(physicalVideoPath))
                {
                    Directory.CreateDirectory(physicalVideoPath);
                }
                var physicalPath = Path.Combine(physicalVideoPath, fullFileName);
                using (var stream = System.IO.File.Create(physicalPath))
                {
                    await file.CopyToAsync(stream);
                }

                //Create media
                var newMedium = new PreviewMedium
                {
                    Name = fileName,
                    FileName = fileGuidName,
                    Extension = fileExtension,
                    Type = FileTypeEnum.Video,
                };
                _context.PreviewMedia.Add(newMedium);
                return newMedium;
            }
            catch
            {
            }
            return null;
        }
    }
}
