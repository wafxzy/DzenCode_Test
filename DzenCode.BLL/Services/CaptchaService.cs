using DzenCode.BLL.Services.Interfaces;
using DzenCode.Common.Entities.DTOs;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DzenCode.BLL.Services
{

    public class CaptchaService : ICaptchaService
    {
        private readonly Dictionary<string, CaptchaData> _captchaStorage = new();
        private readonly Random _random = new();
        private readonly TimeSpan _captchaLifetime = TimeSpan.FromMinutes(5);

        private class CaptchaData
        {
            public string Code { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
        }

        public CaptchaDto GenerateCaptcha()
        {
            CleanExpiredCaptchas();

            string code = GenerateRandomCode();
            string captchaId = Guid.NewGuid().ToString();
            string imageBase64 = GenerateCaptchaImage(code);

            _captchaStorage[captchaId] = new CaptchaData
            {
                Code = code,
                CreatedAt = DateTime.UtcNow
            };

            return new CaptchaDto
            {
                Id = captchaId,
                Image = imageBase64
            };
        }

        public bool ValidateCaptcha(string captchaId, string userInput)
        {
            Console.WriteLine($"Validating CAPTCHA - ID: {captchaId}, UserInput: '{userInput}'");

            if (!_captchaStorage.TryGetValue(captchaId, out var captchaData))
            {
                Console.WriteLine($"CAPTCHA ID not found in storage. Available IDs: {string.Join(", ", _captchaStorage.Keys)}");
                return false;
            }

            Console.WriteLine($"Found CAPTCHA data - Expected: '{captchaData.Code}', CreatedAt: {captchaData.CreatedAt}");

            if (DateTime.UtcNow - captchaData.CreatedAt > _captchaLifetime)
            {
                Console.WriteLine($"CAPTCHA expired. Age: {DateTime.UtcNow - captchaData.CreatedAt}, Lifetime: {_captchaLifetime}");
                _captchaStorage.Remove(captchaId);
                return false;
            }

            bool isValid = string.Equals(captchaData.Code, userInput, StringComparison.OrdinalIgnoreCase);
            Console.WriteLine($"CAPTCHA comparison result: '{captchaData.Code}' == '{userInput}' = {isValid}");

            if (isValid)
                _captchaStorage.Remove(captchaId);

            return isValid;
        }

        private string GenerateRandomCode()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < 5; i++)
            {
                result.Append(chars[_random.Next(chars.Length)]);
            }

            return result.ToString();
        }

        private string GenerateCaptchaImage(string code)
        {
            using Image<Rgba32> image = new Image<Rgba32>(120, 40);

            image.Mutate(ctx =>
            {
                ctx.Fill(Color.White);
                for (int i = 0; i < 50; i++)
                {
                    int x = _random.Next(image.Width);
                    int y = _random.Next(image.Height);
                    Color color = Color.FromRgb((byte)_random.Next(256), (byte)_random.Next(256), (byte)_random.Next(256));
                    image[x, y] = color;
                }
                try
                {
                    Font font = SystemFonts.CreateFont("Arial", 16, FontStyle.Bold);
                    RichTextOptions textOptions = new RichTextOptions(font)
                    {
                        Origin = new PointF(10, 10)
                    };

                    ctx.DrawText(textOptions, code, Color.Black);
                }
                catch
                {
                   Console.WriteLine("Font not available, using default.");
                }

                for (int i = 0; i < 3; i++)
                {
                    int x1 = _random.Next(image.Width);
                    int y1 = _random.Next(image.Height);
                    int x2 = _random.Next(image.Width);
                    int y2 = _random.Next(image.Height);
                    Color color = Color.FromRgb((byte)_random.Next(128), (byte)_random.Next(128), (byte)_random.Next(128));
                    
                    Pen pen = Pens.Solid(color, 1);
                    ctx.DrawLine(pen, new PointF(x1, y1), new PointF(x2, y2));
                }
            });

            using MemoryStream ms = new MemoryStream();
            image.SaveAsPng(ms);
            byte[] bytes = ms.ToArray();
            return Convert.ToBase64String(bytes);
        }

        private void CleanExpiredCaptchas()
        {
            List<string> expiredKeys = _captchaStorage
                .Where(kvp => DateTime.UtcNow - kvp.Value.CreatedAt > _captchaLifetime)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (string key in expiredKeys)
                _captchaStorage.Remove(key);
        }
    }
}
