using DzenCode.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DzenCode.BLL.Services
{

    public class FileService : IFileService
    {
        private readonly string _uploadsPath;
        private readonly string _imagesPath;
        private readonly string _textFilesPath;
        private const int MaxImageWidth = 320;
        private const int MaxImageHeight = 240;
        private const long MaxTextFileSize = 100 * 1024;

        public FileService(IHostEnvironment environment)
        {
            string webRootPath = Path.Combine(environment.ContentRootPath, "wwwroot");
            _uploadsPath = Path.Combine(webRootPath, "uploads");
            _imagesPath = Path.Combine(_uploadsPath, "images");
            _textFilesPath = Path.Combine(_uploadsPath, "textfiles");

            Directory.CreateDirectory(_imagesPath);
            Directory.CreateDirectory(_textFilesPath);
        }

        public async Task<string?> SaveImageAsync(IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                Console.WriteLine("SaveImageAsync: Image is null or empty");
                return null;
            }

            Console.WriteLine($"SaveImageAsync: Processing image {image.FileName}, size: {image.Length}");

            string[] allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            string extension = Path.GetExtension(image.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                Console.WriteLine($"SaveImageAsync: Invalid extension {extension}");
                return null;
            }

            string fileName = $"{Guid.NewGuid()}{extension}";
            string filePath = Path.Combine(_imagesPath, fileName);
            
            Console.WriteLine($"SaveImageAsync: Saving to {filePath}");

            try
            {
                using Stream imageStream = image.OpenReadStream();
                using Image imageSharp = await Image.LoadAsync(imageStream);

                if (imageSharp.Width > MaxImageWidth || imageSharp.Height > MaxImageHeight)
                {
                    double ratio = Math.Min((double)MaxImageWidth / imageSharp.Width,
                                       (double)MaxImageHeight / imageSharp.Height);
                    int newWidth = (int)(imageSharp.Width * ratio);
                    int newHeight = (int)(imageSharp.Height * ratio);

                    imageSharp.Mutate(x => x.Resize(newWidth, newHeight));
                    Console.WriteLine($"SaveImageAsync: Resized to {newWidth}x{newHeight}");
                }

                await imageSharp.SaveAsync(filePath);
                Console.WriteLine($"SaveImageAsync: Successfully saved to {filePath}");
                return $"uploads/images/{fileName}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SaveImageAsync: Error saving image: {ex.Message}");
                return null;
            }
        }

        public async Task<string?> SaveTextFileAsync(IFormFile textFile)
        {
            if (textFile == null || textFile.Length == 0)
                return null;

            if (textFile.Length > MaxTextFileSize)
                return null;

            string extension = Path.GetExtension(textFile.FileName).ToLowerInvariant();
            if (extension != ".txt")
                return null;

            string fileName = $"{Guid.NewGuid()}.txt";
            string filePath = Path.Combine(_textFilesPath, fileName);

            try
            {
                using FileStream stream = new FileStream(filePath, FileMode.Create);
                await textFile.CopyToAsync(stream);
                return $"uploads/textfiles/{fileName}";
            }
            catch
            {
                return null;
            }
        }

        public void DeleteFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            string webRootPath = Path.GetDirectoryName(_uploadsPath) ?? "";
            string fullPath = Path.Combine(webRootPath, filePath);
            if (File.Exists(fullPath))
            {
                try
                {
                    File.Delete(fullPath);
                }
                catch
                {
                    Console.WriteLine($"Failed to delete file: {fullPath}");
                }
            }
        }
    }
}
