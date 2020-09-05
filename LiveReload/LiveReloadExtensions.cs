using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
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
                }

                await next();
            });

            return builder;
        }
    }
}
