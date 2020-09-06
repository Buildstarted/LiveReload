using System.Collections.Generic;
using Microsoft.Extensions.Hosting;

namespace LiveReload
{
    public class LiveReloadOptions
    {
        public string Url { get; set; }
        public List<string> Paths { get; set; }
        public List<string> Extensions { get; set; }
        public bool SaveFormData { get; set; }
        public bool InlineUpdatesWhenPossible { get; set; }
        public List<string> InlineUpdateExtensions { get; set; }
        public bool ReloadOnReconnect { get; set; }
        public bool ShowStatusOnPage { get; set; }
        public string UseFile { get; set; }
        public string LiveReloadLocalScriptPath { get; set; }

        public LiveReloadOptions()
        {
            LiveReloadLocalScriptPath = "/live-reload/live-script.js";
            Url = "/live-reload";
            Paths = new List<string> { "./" };
            Extensions = new List<string> { "cshtml", "css", "js", "html" };
            InlineUpdateExtensions = new List<string> { "jpg", "png", "css", "mp4", "webm" };
            SaveFormData = false;
            InlineUpdatesWhenPossible = true;
            UseFile = null;
            ShowStatusOnPage = false;
            ReloadOnReconnect = false;
        }

        public string ToJavascriptString()
        {
            return "let live_reload_options = {" +
                       $"url: '{Url}'," +
                       $"saveFormData: {(SaveFormData ? "true" : "false")}," +
                       $"showStatusOnPage: {(ShowStatusOnPage ? "true" : "false")}," +
                       $"reloadOnReconnect: {(ReloadOnReconnect ? "true" : "false")}," +
                       $"inlineUpdatesWhenPossible: {(InlineUpdatesWhenPossible ? "true" : "false")}" +
                   "};";
        }
    }
}