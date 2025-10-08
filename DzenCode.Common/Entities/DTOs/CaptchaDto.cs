using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DzenCode.Common.Entities.DTOs
{
    public class CaptchaDto
    {
        public string Image { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
    }
}
