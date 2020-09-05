using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace LiveReload.TagHelpers
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
                if (options.UseFile)
                {
                    output.TagName = "script";
                    output.Attributes.Add("src", "live-reload.js");
                    output.TagMode = TagMode.StartTagAndEndTag;
                }
                else
                {
                    output.TagName = null;
                    output.Content.SetHtmlContent($@"<script>
(() => {{
    let requestUrl = new URL(window.location.href);
    let hostname = requestUrl.hostname;
    let protocol = requestUrl.protocol == 'https:' ? 'wss' : 'ws';

    let socket = new WebSocket(`${{protocol}}://${{hostname}}{options.Url}`);
    socket.onmessage = (e) => {{
        if (e.data.startsWith('reload')) {{
            let path = e.data.split('|')[1];
            let link = document.createElement('link');
            link.setAttribute('href', path);
            link.setAttribute('rel', 'stylesheet');
            link.setAttribute('type', 'text/css');

            document.head.appendChild(link);
        }} else {{
            window.location.href = window.location.href;
        }}
    }};

//socket.onopen = (e) => {{ console.log('opened', e); }};
//socket.onclose = (e) => {{ console.log('close', e); }};
//socket.onerror = (e) => {{ console.log('error', e); }};
}})();
</script>");
                }
            }
            catch (Exception e)
            {
                Debugger.Break();
            }
        }
    }
}