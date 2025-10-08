﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DzenCode.BLL.Services.Interfaces
{
    public interface IFileService
    {
        Task<string?> SaveImageAsync(IFormFile image);
        Task<string?> SaveTextFileAsync(IFormFile textFile);
        void DeleteFile(string filePath);
    }

}
