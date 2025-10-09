using DzenCode.BLL.Services;
using DzenCode.BLL.Services.Interfaces;
using Microsoft.Extensions.Hosting;

namespace DzenCodeTest.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddServiceExtensions(this IServiceCollection Services)
        {
            Services.AddSingleton<ICaptchaService, CaptchaService>();
            Services.AddScoped<IFileService, FileService>();
            Services.AddScoped<IHtmlSanitizerService, HtmlSanitizerService>();

            return Services;

        }
    }
}
