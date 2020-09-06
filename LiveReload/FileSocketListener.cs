using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LiveReload
{
    public class FileSocketListener : IDisposable
    {
        readonly Dictionary<int, DateTime> filetracker;
        private WebSocket socket;
        private ArraySegment<byte> refresh;
        private readonly SemaphoreSlim locker;

        public FileSocketListener(WebSocket socket)
        {
            this.socket = socket;
            filetracker = new Dictionary<int, DateTime>();
            refresh = new ArraySegment<byte>(Encoding.ASCII.GetBytes("refresh"));
            locker = new SemaphoreSlim(1, 1);
        }

        public void Update(string filename, bool inline)
        {
            if (!IsOpen) return;

            lock (filetracker)
            {
                var hash = filename.GetHashCode();
                if (!filetracker.ContainsKey(hash))
                {
                    filetracker.Add(hash, DateTime.MinValue);
                }

                if (filetracker[hash].AddMilliseconds(100) < DateTime.UtcNow)
                {
                    filetracker[hash] = DateTime.UtcNow;

                    if (inline)
                    {
                        var buffer = Encoding.UTF8.GetBytes(filename);

                        socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, default);
                    }
                    else
                    {
                        socket.SendAsync(refresh, WebSocketMessageType.Text, true, default);
                    }
                }
            }
        }
        
        public bool IsOpen => socket.State == WebSocketState.Open;

        public void Dispose()
        {
            socket?.Dispose();
            locker?.Dispose();
        }

        public async Task Receive()
        {
            try
            {
                var buffer = new ArraySegment<byte>(new byte[16]);
                await socket.ReceiveAsync(buffer, CancellationToken.None);
            }
            catch (WebSocketException)
            {
                //do nothing let the connection close
            }
        }
    }
}