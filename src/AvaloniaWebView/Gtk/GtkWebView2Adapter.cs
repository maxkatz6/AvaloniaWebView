#if !WINDOWS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Avalonia.Controls.Platform;
using Avalonia.Input;
using Avalonia.Platform;
using Avalonia.Platform.Interop;

using static AvaloniaWebView.GlibInterop;
using static AvaloniaWebView.GtkInterop;

namespace AvaloniaWebView;

internal class GtkWebView2Adapter : IWebViewAdapter, IDisposable
{
    private readonly WebKit.WebView _webView;
    private static Task<IntPtr>? s_gtkTask;

    public GtkWebView2Adapter()
    {
        if (s_gtkTask == null)
            s_gtkTask = StartGtk();
        var appHandle = s_gtkTask.Result;
        if (appHandle == default)
            throw new InvalidOperationException("Unable to start GTK app");

        (_webView, PlatformHandle) = RunOnGlibThread(() =>
        {
            var app = new Gtk.Application(appHandle);

            var window = new Gtk.Window(Gtk.WindowType.Toplevel);
            window.Title = nameof(GtkWebView2Adapter);
            window.KeepAbove = true;
            app.AddWindow(window);

            var webView = new WebKit.WebView();
            webView.Realize();
            window.Add(webView);
            window.ShowAll();
            window.Present();
            return (webView, new GtkHandle(webView, window));
        }).Result;
    }

    public IPlatformHandle PlatformHandle { get; }

    public bool CanGoBack => RunOnGlibThread(() => _webView.CanGoBack()).Result;

    public bool CanGoForward => RunOnGlibThread(() => _webView.CanGoForward()).Result;

    public Uri? Source
    {
        get => RunOnGlibThread(() => new Uri(_webView.Uri)).Result;
        set => RunOnGlibThread(() => _webView.LoadUri(value?.AbsolutePath)).Wait();
    }

    public event EventHandler<WebViewNavigationEventArgs>? NavigationCompleted;
    public event EventHandler<WebViewNavigationEventArgs>? NavigationStarted;

    public void Dispose()
    {
        RunOnGlibThread(() =>
        {
            _webView.Window?.Dispose();
            _webView.Dispose();
        }).Wait();
    }

    public bool GoBack()
    {
        return RunOnGlibThread(() =>
        {
            _webView.GoBack();
            return true;
        }).Result;
    }

    public bool GoForward()
    {
        return RunOnGlibThread(() =>
        {
            _webView.GoForward();
            return true;
        }).Result;
    }

    public bool HandleKeyDown(Key key, KeyModifiers keyModifiers)
    {
        return false;
    }

    public void HandleResize(int width, int height, float zoom)
    {
    }

    public string? InvokeScript(string scriptName)
    {
        return RunOnGlibThread(() =>
        {
            _webView.RunJavascript(scriptName);
            return (string?)null;
        }).Result;
    }

    public void Navigate(Uri url)
    {
        RunOnGlibThread(() => _webView.LoadUri(url.AbsolutePath)).Wait();
    }

    public Task NavigateToString(string text)
    {
        return RunOnGlibThread(() =>
        {
            _webView.LoadHtml(text);
        });
    }

    public void Refresh()
    {
        RunOnGlibThread(() => _webView.Reload()).Wait();
    }

    public void Stop()
    {
        RunOnGlibThread(() => _webView.StopLoading()).Wait();
    }
}

internal class GtkHandle : INativeControlHostDestroyableControlHandle
{
    private readonly Gtk.Widget _widget;
    private readonly Gtk.Window _window;

    public GtkHandle(Gtk.Widget widget, Gtk.Window window)
    {
        _window = window;
        _widget = widget;

        Handle = gdk_x11_window_get_xid(window.Handle);
    }

    public IntPtr Handle { get; }
    public string HandleDescriptor => "XID";
    public void Destroy()
    {
        RunOnGlibThread(() =>
        {
            _widget.Destroy();
            _window.Destroy();
            return 0;
        }).Wait();
    }
}
#endif
