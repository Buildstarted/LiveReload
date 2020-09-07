# LiveReload

Automatically refresh pages when you make changes to cshtml, js, css files.

### Startup.cs
```
public void ConfigureServices(IServiceCollection services)
{
  services.AddLiveReload();
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
  app.UseLiveReload();
}
```

### _ViewImports.cshtml
```
@addTagHelper *, LiveReload
```

### _Layout.cshtml
at the bottom of your cshtml file

```
  <live-reload />
```


### Options
There are several options to make your life easier.

```services.AddLiveReload(options => { ... });```

1. `Url` (default `/live-reload`): You can change this websocket url to anything you want so as not to conflict with anything you already have.
2. `Paths.Add("...");` (default current root): Add as many paths as you'd like to watch.
3. `Extensions.Add("myext");` (default `cshtml, css, js, html`): Add more extensions to monitor for changes.
4. `InlineUpdateExtensions.Add("jpg");` (default `jpg, png, css, mp4, webm`): This will try and update the element on the page without refreshing the whole page.
5. `SaveFormData` (default `false`): Attempt save any forms on your page so that on full refresh the data is restored saving you some work.
6. `InlineUpdatesWhenPossible` (default `true`): Attempt to update elements without refreshing the whole page such as images and css.
7. `ShowStatusOnPage` (default `false`): Shows a little status icon in the upper right that signals whether or not the page is listening for changes.
8. `ReloadOnReconnect` (default `false`): Reload the page when the service is reconnect. This is helpful when you stop debugging and start again.
9. `UseFile` (default `null`): Use a different file for automatic reloading. Not that useful unless you have a custom script.
10. `LiveReloadLocalScriptPath` (default `/live-reload/live-script.js`): Change this if you have an existing script that conflicts with this path.
