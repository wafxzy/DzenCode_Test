using AutoMapper;
using DzenCode.BLL.Services.Interfaces;
using DzenCode.Common.Entities;
using DzenCode.Common.Entities.DTOs;
using DzenCode.DAL.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DzenCodeTest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly CommentsDBContext _context;
        private readonly IMapper _mapper;
        private readonly ICaptchaService _captchaService;
        private readonly IFileService _fileService;
        private readonly IHtmlSanitizerService _htmlSanitizer;

        public CommentsController(
            CommentsDBContext context,
            IMapper mapper,
            ICaptchaService captchaService,
            IFileService fileService,
            IHtmlSanitizerService htmlSanitizer)
        {
            _context = context;
            _mapper = mapper;
            _captchaService = captchaService;
            _fileService = fileService;
            _htmlSanitizer = htmlSanitizer;
        }

        [HttpGet]
        public async Task<ActionResult<object>> GetComments(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 25,
            [FromQuery] string sortBy = "CreatedAt",
            [FromQuery] string sortOrder = "desc")
        {

            IQueryable<Comment> baseQuery = _context.Comments
                .Where(c => c.ParentId == null);

            IQueryable<Comment> query = sortBy.ToLowerInvariant() switch
            {
                "username" => sortOrder.ToLowerInvariant() == "asc"
                    ? baseQuery.OrderBy(c => c.UserName)
                    : baseQuery.OrderByDescending(c => c.UserName),
                "email" => sortOrder.ToLowerInvariant() == "asc"
                    ? baseQuery.OrderBy(c => c.Email)
                    : baseQuery.OrderByDescending(c => c.Email),
                _ => sortOrder.ToLowerInvariant() == "asc"
                    ? baseQuery.OrderBy(c => c.CreatedAt)
                    : baseQuery.OrderByDescending(c => c.CreatedAt)
            };

            int totalCount = await query.CountAsync();
            List<Comment> rootComments = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            foreach (Comment comment in rootComments)
            {
                await LoadRepliesRecursively(comment);
            }

            List<CommentResponseDto> result = _mapper.Map<List<CommentResponseDto>>(rootComments);

            foreach (CommentResponseDto dto in result)
            {
                MakePathsAbsoluteRecursive(dto);
            }

            return Ok(new
            {
                comments = result,
                totalCount,
                currentPage = page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }

        private async Task LoadRepliesRecursively(Comment comment)
        {
            comment.Replies = await _context.Comments
                .Where(c => c.ParentId == comment.Id)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            foreach (Comment reply in comment.Replies)
            {
                await LoadRepliesRecursively(reply);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CommentResponseDto>> GetComment(int id)
        {
            Comment? comment = await _context.Comments
                .Include(c => c.Replies)
                .ThenInclude(r => r.Replies)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null)
                return NotFound();

            CommentResponseDto dto = _mapper.Map<CommentResponseDto>(comment);
            MakePathsAbsoluteRecursive(dto);
            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<CommentResponseDto>> CreateComment([FromForm] CommentDto commentDto, [FromForm] string captchaId)
        {
            Console.WriteLine($"Received comment: UserName={commentDto?.UserName}, Email={commentDto?.Email}, CaptchaCode={commentDto?.CaptchaCode}, CaptchaId={captchaId}");

            if (commentDto == null)
                return BadRequest("Comment data is required");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is invalid:");
                foreach (KeyValuePair<string, Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateEntry> error in ModelState)
                {
                    Console.WriteLine($"Key: {error.Key}, Errors: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
                return BadRequest(ModelState);
            }

            // Validate CAPTCHA
            bool captchaValid = _captchaService.ValidateCaptcha(captchaId, commentDto.CaptchaCode);
            Console.WriteLine($"CAPTCHA validation result: {captchaValid} for captchaId={captchaId}, code={commentDto.CaptchaCode}");

            if (!captchaValid)
                return BadRequest("Invalid CAPTCHA");

            if (commentDto.ParentId.HasValue)
            {
                Comment? parentComment = await _context.Comments
                    .FirstOrDefaultAsync(c => c.Id == commentDto.ParentId.Value);
                if (parentComment == null)
                    return BadRequest("Parent comment not found");
            }

            Comment comment = _mapper.Map<Comment>(commentDto);

            comment.Text = _htmlSanitizer.SanitizeHtml(comment.Text);

            if (commentDto.Image != null)
            {
                Console.WriteLine($"Processing image upload: {commentDto.Image.FileName}");
                comment.ImagePath = await _fileService.SaveImageAsync(commentDto.Image);
                Console.WriteLine($"Image saved with path: {comment.ImagePath}");
            }

            if (commentDto.TextFile != null)
            {
                Console.WriteLine($"Processing text file upload: {commentDto.TextFile.FileName}");
                comment.TextFilePath = await _fileService.SaveTextFileAsync(commentDto.TextFile);
                Console.WriteLine($"Text file saved with path: {comment.TextFilePath}");
            }

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            Comment? createdComment = await _context.Comments
                .Include(c => c.Replies)
                .FirstOrDefaultAsync(c => c.Id == comment.Id);

            CommentResponseDto createdDto = _mapper.Map<CommentResponseDto>(createdComment!);
            MakePathsAbsoluteRecursive(createdDto);

            return CreatedAtAction(nameof(GetComment),
                new { id = comment.Id },
                createdDto);
        }

        [HttpPost("preview")]
        public ActionResult<string> PreviewComment([FromBody] string text)
        {
            if (string.IsNullOrEmpty(text))
                return BadRequest("Text is required");

            string sanitizedText = _htmlSanitizer.SanitizeHtml(text);
            return Ok(sanitizedText);
        }

        private void MakePathsAbsoluteRecursive(CommentResponseDto dto)
        {
            dto.ImagePath = ToAbsolute(dto.ImagePath);
            dto.TextFilePath = ToAbsolute(dto.TextFilePath);
            if (dto.Replies != null)
            {
                foreach (CommentResponseDto child in dto.Replies)
                {
                    MakePathsAbsoluteRecursive(child);
                }
            }
        }

        private string? ToAbsolute(string? relative)
        {
            if (string.IsNullOrWhiteSpace(relative)) return relative;
            if (Uri.TryCreate(relative, UriKind.Absolute, out _)) return relative;
            string path = relative.StartsWith("/") ? relative : "/" + relative;
            string scheme = Request.Scheme;
            string host = Request.Host.Value ?? "localhost";
            return $"{scheme}://{host}{path}";
        }

        [HttpGet("test-images")]
        public ActionResult TestImages()
        {
            string imagesPath = Path.Combine("wwwroot", "uploads", "images");
            List<object> files = Directory.GetFiles(imagesPath)
                .Select(f => new { 
                    fileName = Path.GetFileName(f),
                    url = ToAbsolute($"/uploads/images/{Path.GetFileName(f)}")
                })
                .ToList<object>();
            
            return Ok(files);
        }
    }
}
