using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;

namespace LiveReload
{
    public class LiveReloadWatcher
    {
        private readonly IHostingEnvironment env;
        private readonly LiveReloadOptions options;
        private bool started;
        private List<FileSystemWatcher> watchers;

        public event OnChangedDelegate OnChanged;
        public delegate void OnChangedDelegate(string filename, bool inlinereload);

        public LiveReloadWatcher(IOptions<LiveReloadOptions> options, IHostingEnvironment env)
        {
            this.env = env;
            this.options = options.Value;
            started = false;
            watchers = new List<FileSystemWatcher>();
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
                    watchers.Add(watcher);
                }
            }
        }

        private void OnChangedImpl(object sender, FileSystemEventArgs args)
        {
            foreach (var extension in options.Extensions)
            {
                if (args.FullPath.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                {
                    var index = 0;
                    if (args.FullPath.StartsWith(env.WebRootPath))
                    {
                        index = env.WebRootPath.Length;
                    }

                    OnChanged?.Invoke(args.FullPath.Substring(0, index), options.InlineUpdateExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase));
                    return;
                }
            }
        }

        public async Task Handle(FileSocketListener listener)
        {
            OnChanged += listener.Update;

            while (listener.IsOpen)
            {
                await listener.Receive();
            }

            OnChanged -= listener.Update;
        }
    }
}