using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace LiveReload
{
    [HtmlTargetElement("live-reload", TagStructure = TagStructure.WithoutEndTag)]
    public class LiveReloadTagHelper : TagHelper
    {
        private LiveReloadOptions options;

        public LiveReloadTagHelper(IOptions<LiveReloadOptions> options)
        {
            this.options = options.Value;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            try
            {
                string randomid = "";
#if LIVE_RELOAD_DEV
                randomid = "?" + Guid.NewGuid().ToString().Substring(0, 8);
#endif

                output.TagName = null;
                output.Content.SetHtmlContent($"<script>{options.ToJavascriptString()}</script><script src='{options.LiveReloadLocalScriptPath}{randomid}'></script>");
            }
            catch (Exception e)
            {
                Debugger.Break();
            }
        }
    }
}