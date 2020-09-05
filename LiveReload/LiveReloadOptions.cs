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
#if LIVE_RELOAD_DEV
        public string UseFile { get; set; }
#endif

        public LiveReloadOptions()
        {
            Url = "/live-reload";
            Paths = new List<string> { "./" };
            Extensions = new List<string> { "cshtml", "css", "js" };
            InlineUpdateExtensions = new List<string> { "jpg", "png", "css", "mp4", "webm" };
            SaveFormData = false;
            InlineUpdatesWhenPossible = true;
            UseFile = null;
        }
    }
}