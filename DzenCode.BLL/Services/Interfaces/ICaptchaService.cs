using DzenCode.Common.Entities.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DzenCode.BLL.Services.Interfaces
{
    public interface ICaptchaService
    {
        CaptchaDto GenerateCaptcha();
        bool ValidateCaptcha(string captchaId, string userInput);
    }
}
