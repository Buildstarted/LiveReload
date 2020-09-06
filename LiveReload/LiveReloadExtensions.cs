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
                var options = builder.ApplicationServices.GetService<IOptions<LiveReloadOptions>>();
                if (options?.Value != null)
                {
                    if (context.Request.Path == options.Value.LiveReloadLocalScriptPath)
                    {
                        if (options.Value.UseFile != null)
                        {
                            var path = options.Value.UseFile;
                            if (File.Exists(path))
                            {
                                context.Response.ContentType = "application/javascript";
                                await context.Response.SendFileAsync(path);
                            }
                            else
                            {
                                context.Response.StatusCode = 404;
                            }

                            return;
                        }

                        context.Response.ContentType = "application/javascript";
                        await context.Response.WriteAsync(Properties.Resources.live_reload);
                        return;
                    }
                }

                await next();
            });

            builder.Use(async (context, next) =>
            {
                var options = builder.ApplicationServices.GetService<IOptions<LiveReloadOptions>>();
                if (options?.Value != null)
                {
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
                            using var socket = await context.WebSockets.AcceptWebSocketAsync();
                            using var listener = new FileSocketListener(socket);
                            await livereloadwatcher.Handle(listener);
                        }
                        else
                        {
                            context.Response.StatusCode = 400;
                        }

                        return;
                    }
                }

                await next();
            });

            return builder;
        }
    }
}
