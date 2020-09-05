using System;
using System.Diagnostics;
using System.Threading.Tasks;
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
                output.TagName = null;
                output.Content.SetHtmlContent(Properties.Resources.live_reload.Replace("/live-reload", options.Url));
            }
            catch (Exception e)
            {
                Debugger.Break();
            }
        }
    }
}