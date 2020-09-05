using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace LiveReload
{
    public class LiveReloadWatcher
    {
        private readonly LiveReloadOptions options;
        private bool started;
        private readonly ArraySegment<byte> refresh;

        public LiveReloadWatcher(IOptions<LiveReloadOptions> options)
        {
            this.options = options.Value;
            started = false;
            this.refresh = new ArraySegment<byte>(Encoding.ASCII.GetBytes("refresh"));
        }

        public void Start()
        {
            lock (this)
            {
                if (started) return;
                started = true;
            }

            foreach (var path in options.Paths)
            {
                var rootpath = Path.Combine(Environment.CurrentDirectory, path);

                if (Directory.Exists(rootpath))
                {
                    var watcher = new System.IO.FileSystemWatcher(rootpath, "*");
                    watcher.EnableRaisingEvents = true;
                    watcher.IncludeSubdirectories = true;
                    watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                    watcher.Changed += OnChangedImpl;
                    watcher.Created += OnChangedImpl;
                    watcher.Deleted += OnChangedImpl;
                    watcher.Renamed += OnChangedImpl;
                }
            }
        }

        private async void OnChangedImpl(object sender, FileSystemEventArgs args)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(150));

            var path = args.FullPath;
            foreach (var extension in options.Extensions)
            {
                if (path.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                {
                    var name = args.Name;
                    if (args.Name.StartsWith("wwwroot"))
                    {
                        name = args.Name.Substring(args.Name.IndexOf("\\") + 1);
                    }

                    OnChanged?.Invoke(name + $"?{Guid.NewGuid().ToString()}", extension == "css");
                    return;
                }
            }
        }

        public delegate Task OnChangedDelegate(string path, bool inlinereload);

        public event OnChangedDelegate OnChanged;

        public async Task Handle(WebSocket socket)
        {
            var tcs = new TaskCompletionSource<bool>();
            OnChanged += async (path, inlinereload) =>
            {
                if (inlinereload)
                {
                    await socket.SendAsync(new ArraySegment<byte>(Encoding.ASCII.GetBytes($"reload|{path}")), WebSocketMessageType.Text, true, default);
                }
                else
                {
                    await socket.SendAsync(refresh, WebSocketMessageType.Text, true, default);
                }
            };

            var buffer = new ArraySegment<byte>(new byte[512]);

            while (socket.State == WebSocketState.Open)
            {
                try
                {
                    var msg = await socket.ReceiveAsync(buffer, CancellationToken.None);
                }
                catch { }
            }
        }
    }
}