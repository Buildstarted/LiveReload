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
        private byte[] buffer;
        private readonly SemaphoreSlim locker;
        private static readonly Random random = new Random();
        private ArraySegment<byte> ping;

        public FileSocketListener(WebSocket socket)
        {
            this.socket = socket;
            filetracker = new Dictionary<int, DateTime>();
            refresh = new ArraySegment<byte>(Encoding.ASCII.GetBytes("refresh"));
            ping = new ArraySegment<byte>(Encoding.ASCII.GetBytes("ping"));
            buffer = new byte[1024];
            locker = new SemaphoreSlim(1, 1);
        }

        public void Update(int hash, ReadOnlySpan<char> filename, bool inline)
        {
            if (!IsOpen) return;

            lock (filetracker)
            {
                if (!filetracker.ContainsKey(hash))
                {
                    filetracker.Add(hash, DateTime.MinValue);
                }

                if (filetracker[hash].AddMilliseconds(100) < DateTime.UtcNow)
                {
                    filetracker[hash] = DateTime.UtcNow;

                    if (inline)
                    {
                        var written = WriteMessage(filename, buffer);

                        socket.SendAsync(new ArraySegment<byte>(buffer, 0, written), WebSocketMessageType.Text, true, default);
                    }
                    else
                    {
                        socket.SendAsync(refresh, WebSocketMessageType.Text, true, default);
                    }
                }
            }
        }

        private int WriteMessage(ReadOnlySpan<char> filename, byte[] buffer)
        {
            const int reloadlength = 7;
            const int hashlength = 9; //including ?

            buffer[0] = (byte)'r';
            buffer[1] = (byte)'e';
            buffer[2] = (byte)'l';
            buffer[3] = (byte)'o';
            buffer[4] = (byte)'a';
            buffer[5] = (byte)'d';
            buffer[6] = (byte)'|';

            for (var i = 0; i < filename.Length; i++)
            {
                buffer[i + reloadlength] = (byte)(filename[i] == Path.DirectorySeparatorChar ? (byte)'/' : (byte)filename[i]);
            }

            buffer[reloadlength + filename.Length] = (byte)'?';
            for (var i = 0; i < 8; i++)
            {
                buffer[reloadlength + 1 + filename.Length + i] = (byte) (random.Next(0, 26) + 97);
            }

            return reloadlength + filename.Length + hashlength;
        }

        public bool IsOpen => socket.State == WebSocketState.Open;

        public void Dispose()
        {
            socket?.Dispose();
            locker?.Dispose();
        }

        public async Task SendPing()
        {
            var ct = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await socket.SendAsync(ping, WebSocketMessageType.Text, true, ct.Token);
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