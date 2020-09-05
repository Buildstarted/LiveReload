namespace LiveReload
{
    public class LiveReloadOptions
    {
        public string Url { get; set; }
        public string[] Paths { get; set; }
        public string[] Extensions { get; set; }
#if LOCALDEV
        public bool UseFile { get; set; }
#endif

        public LiveReloadOptions()
        {
            Url = "/live-reload";
            Paths = new[] { "./" };
            Extensions = new[] { "cshtml", "css", "js" };
        }
    }
}