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

        [HtmlAttributeName("show-status-on-page")]
        public bool? ShowStatusOnPage { get; set; }
        [HtmlAttributeName("reload-on-reconnect")]
        public bool? ReloadOnReconnect { get; set; }
        [HtmlAttributeName("inline-updates-when-possible")]
        public bool? InlineUpdatesWhenPossible { get; set; }
        [HtmlAttributeName("save-form-data")]
        public bool? SaveFormData { get; set; }


        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            try
            {
                string randomid = "";
#if LIVE_RELOAD_DEV
                randomid = "?" + Guid.NewGuid().ToString().Substring(0, 8);
#endif

                //options override
                //show status
                //Show-Status-On-Page
                var localoptions = new LiveReloadOptions()
                {
                    SaveFormData = SaveFormData ?? options.SaveFormData,
                    ReloadOnReconnect = ReloadOnReconnect ?? options.ReloadOnReconnect,
                    ShowStatusOnPage = ShowStatusOnPage ?? options.ShowStatusOnPage,
                    InlineUpdatesWhenPossible = InlineUpdatesWhenPossible ?? options.InlineUpdatesWhenPossible
                };
                
                output.TagName = null;
                output.Content.SetHtmlContent($"<script>{localoptions.ToJavascriptString()}</script><script src='{options.LiveReloadLocalScriptPath}{randomid}'></script>");
            }
            catch (Exception e)
            {
                Debugger.Break();
            }
        }
    }
}