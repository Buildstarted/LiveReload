using System.Collections.Generic;

namespace LiveReload
{
    public class LiveReloadOptions
    {
        public string Url { get; set; }
        public List<string> Paths { get; set; }
        public List<string> Extensions { get; set; }
#if LOCALDEV
        public bool UseFile { get; set; }
#endif

        public LiveReloadOptions()
        {
            Url = "/live-reload";
            Paths = new List<string> { "./" };
            Extensions = new List<string> { "cshtml", "css", "js" };
        }
    }
}