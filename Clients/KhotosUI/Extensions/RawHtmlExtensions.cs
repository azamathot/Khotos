using Microsoft.AspNetCore.Components;

namespace KhotosUI.Extensions
{
    public static class RawHtmlExtensions
    {
        public static MarkupString ToRawHtml(this string html)
        {
            return (MarkupString)html;
        }
    }
}
