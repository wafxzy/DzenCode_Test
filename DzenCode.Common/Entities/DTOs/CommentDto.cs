using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DzenCode.Common.Entities.DTOs
{
    public class CommentDto
    {
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "User Name can only contain Latin letters and numbers")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Url]
        public string? HomePage { get; set; }

        [Required]
        public string Text { get; set; } = string.Empty;

        public int? ParentId { get; set; }

        [Required]
        public string CaptchaCode { get; set; } = string.Empty;

        public IFormFile? Image { get; set; }
        public IFormFile? TextFile { get; set; }
    }
}
