namespace LiveReload
{
    public class LiveReloadOptions
    {
        public string Url { get; set; }
        public string Path { get; set; }
        public string[] Extensions { get; set; }
#if LOCALDEV
        public bool UseFile { get; set; }
#endif

        public LiveReloadOptions()
        {
            Url = "/live-reload";
            Path = "./";
            Extensions = new[] { "cshtml", "css", "js" };
        }
    }
}