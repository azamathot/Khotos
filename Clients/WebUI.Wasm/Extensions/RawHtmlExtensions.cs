using Microsoft.AspNetCore.Components;

namespace WebUI_Wasm.Extensions
{
    public static class RawHtmlExtensions
    {
        public static MarkupString ToRawHtml(this string html)
        {
            return (MarkupString)html;
        }
    }
}
