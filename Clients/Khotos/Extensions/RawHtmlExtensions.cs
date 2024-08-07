using Microsoft.AspNetCore.Components;

namespace Khotos.Extensions
{
    public static class RawHtmlExtensions
    {
        public static MarkupString ToRawHtml(this string html)
        {
            return (MarkupString)html;
        }
    }
}
