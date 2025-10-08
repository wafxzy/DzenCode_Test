using DzenCode.BLL.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace DzenCode.BLL.Services
{

    public class HtmlSanitizerService : IHtmlSanitizerService
    {
        private readonly HashSet<string> _allowedTags = new(StringComparer.OrdinalIgnoreCase)
        {
            "a", "code", "i", "strong"
        };

        private readonly Dictionary<string, HashSet<string>> _allowedAttributes = new(StringComparer.OrdinalIgnoreCase)
        {
            ["a"] = new(StringComparer.OrdinalIgnoreCase) { "href", "title" }
        };

        public string SanitizeHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;
            var encoded = HttpUtility.HtmlEncode(html);
            encoded = DecodeAllowedTags(encoded);
            if (!IsValidXhtml(encoded))
                return HttpUtility.HtmlEncode(html); 

            return encoded;
        }

        private string DecodeAllowedTags(string html)
        {
            foreach (var tag in _allowedTags)
            {
                html = Regex.Replace(html,
                    $@"&lt;{tag}&gt;",
                    $"<{tag}>",
                    RegexOptions.IgnoreCase);

                html = Regex.Replace(html,
                    $@"&lt;/{tag}&gt;",
                    $"</{tag}>",
                    RegexOptions.IgnoreCase);

                if (_allowedAttributes.ContainsKey(tag))
                {
                    var allowedAttrs = _allowedAttributes[tag];
                    var attrPattern = string.Join("|", allowedAttrs);

                    html = Regex.Replace(html,
                        $@"&lt;{tag}\s+({attrPattern})\s*=\s*&quot;([^&]*)&quot;&gt;",
                        match => {
                            var attr = match.Groups[1].Value;
                            var value = match.Groups[2].Value;
                            if (attr.Equals("href", StringComparison.OrdinalIgnoreCase))
                            {
                                if (!IsValidUrl(value))
                                    return match.Value;
                            }

                            return $"<{tag} {attr}=\"{HttpUtility.HtmlEncode(value)}\">";
                        },
                        RegexOptions.IgnoreCase);
                }
            }

            return html;
        }

        private bool IsValidXhtml(string html)
        {
            var stack = new Stack<string>();
            var tagPattern = @"<(/?)(\w+)(?:\s+[^>]*)?>";

            var matches = Regex.Matches(html, tagPattern, RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                var isClosing = !string.IsNullOrEmpty(match.Groups[1].Value);
                var tagName = match.Groups[2].Value.ToLowerInvariant();

                if (!_allowedTags.Contains(tagName))
                    return false;

                if (isClosing)
                {
                    if (stack.Count == 0 || stack.Pop() != tagName)
                        return false;
                }
                else
                {
                    stack.Push(tagName);
                }
            }

            return stack.Count == 0;
        }

        private bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                   (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }
    }
}
