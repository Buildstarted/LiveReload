using System;
using System.IO;
using System.Net.Mime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LiveReload
{
    public static class LiveReloadExtensions
    {
        public static IServiceCollection AddLiveReload(this IServiceCollection services)
        {
            services.AddSingleton<LiveReloadWatcher>();
            services.AddSingleton<LiveReloadOptions>();

            return services;
        }

        public static IServiceCollection AddLiveReload(this IServiceCollection services, Action<LiveReloadOptions> configure)
        {
            AddLiveReload(services);

            services.Configure(configure);

            return services;
        }

        public static IApplicationBuilder UseLiveReload(this IApplicationBuilder builder)
        {
            builder.UseWebSockets();

            builder.Use(async (context, next) =>
            {
                if (context.Request.Path == LiveReloadTagHelper.LiveReloadLocalScriptPath)
                {
                    var options = builder.ApplicationServices.GetService<IOptions<LiveReloadOptions>>();
#if LOCALDEV
                    if (options.Value.UseFile)
                    {
                        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "live-reload.js");
                        if (File.Exists(path))
                        {
                            context.Response.ContentType = "application/javascript";
                            using var file = File.OpenRead(path);
                            var buffer = new byte[1024];
                            var read = -1;
                            while ((read = await file.ReadAsync(buffer, 0, 1024)) != 0)
                            {
                                await context.Response.Body.WriteAsync(buffer, 0, read);
                            }

                            return;
                        }
                    }
#endif

                    context.Response.ContentType = "application/javascript";
                    await context.Response.WriteAsync(Properties.Resources.live_reload);
                    return;
                }

                await next();
            });

            builder.Use(async (context, next) =>
            {
                var options = builder.ApplicationServices.GetService<IOptions<LiveReloadOptions>>();
                if (context.Request.Path == options.Value.Url)
                {
                    var livereloadwatcher = context.RequestServices.GetService<LiveReloadWatcher>();
                    if (livereloadwatcher == null)
                    {
                        throw new InvalidOperationException("");
                    }
                    livereloadwatcher.Start();

                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        Console.WriteLine($"[{DateTime.Now}] Connection started");
                        var socket = await context.WebSockets.AcceptWebSocketAsync();
                        await livereloadwatcher.Handle(socket);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }

                    return;
                }

                await next();
            });

            return builder;
        }
    }
}
