using DzenCode.BLL.Services.Interfaces;
using DzenCode.Common.Entities.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DzenCodeTest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CaptchaController : ControllerBase
    {
        private readonly ICaptchaService _captchaService;

        public CaptchaController(ICaptchaService captchaService)
        {
            _captchaService = captchaService;
        }

        [HttpGet]
        public ActionResult<CaptchaDto> GenerateCaptcha()
        {
            CaptchaDto captcha = _captchaService.GenerateCaptcha();
            return Ok(captcha);
        }
    }
}
