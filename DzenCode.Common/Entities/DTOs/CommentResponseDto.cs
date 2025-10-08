using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DzenCode.Common.Entities.DTOs
{
    public class CommentResponseDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? HomePage { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int? ParentId { get; set; }
        public List<CommentResponseDto> Replies { get; set; } = new();
        public string? ImagePath { get; set; }
        public string? TextFilePath { get; set; }
    }
}
