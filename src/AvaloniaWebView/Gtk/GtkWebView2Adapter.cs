#if DISABLE
using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.X11.Interop;
using GLib;
using Gtk;

using static Avalonia.X11.Interop.GtkInteropHelper;
using Application = Gtk.Application;

namespace AvaloniaWebView.Gtk;

[SupportedOSPlatform("linux")]
internal class GtkWebView2Adapter : IWebViewAdapter
{
    private WebKit.WebView _webView;
    private global::Gtk.Window _window;
    private static Task<IntPtr>? s_gtkTask;
    private readonly SynchronizationContext? _gtkContext;

    [DllImport("libgdk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr gdk_x11_window_get_xid(IntPtr window);

    public GtkWebView2Adapter()
    {
        RunOnGlibThread(() =>
        {
            var window = new Window(WindowType.Toplevel);


            var webView = new WebKit.WebView();
            webView.LoadUri("http://www.google.com/");
            window.Add(webView);

            window.ShowAll();

            //var window = new global::Gtk.Window(nameof(GtkWebView2Adapter));
            //window.KeepAbove = true;

            //// ((Application)GLib.Application.Default).AddWindow(window);

            //var webView = new WebKit.WebView();
            //var a = WebKit.Global.IsSupported;
            //Fixed fix = new Fixed();

            //Button btn1 = new Button("Button");
            //btn1.Sensitive = false;
            //Button btn2 = new Button("Button");
            //Button btn3 = new Button(Stock.Close);
            //Button btn4 = new Button("Button");
            //webView.SetSizeRequest(80, 40);

            //fix.Put(btn1, 20, 30);
            //fix.Put(btn2, 100, 30);
            //fix.Put(btn3, 20, 80);
            //fix.Put(btn4, 100, 80);

            //window.Add(webView);


            //Button btn = new Button("Hello World");
            //webView.LoadUri("http://www.google.com/");
            //// window.Add(webView);
            //window.Realize();
            ////window.Add(btn);
            //window.SetDefaultSize(500, 500);
            //window.SetPosition(WindowPosition.Center);
            //window.ShowAll();
            //window.Present();

            // var handle = gdk_x11_window_get_xid(window.GdkWindow.Handle);
            return 0;
        });
    }

    public IntPtr Handle { get; private set; }
    public string HandleDescriptor => "XID";

    public bool IsInitialized => true;
    public void SizeChanged() {  }

    public bool CanGoBack => RunOnGlibThread(() => _webView.CanGoBack()).Result;

    public bool CanGoForward => RunOnGlibThread(() => _webView.CanGoForward()).Result;

    public Uri Source
    {
        get => RunOnGlibThread(() => new Uri(_webView.Uri)).Result;
        set => RunOnGlibThread(() => { _webView.LoadUri(value?.AbsolutePath); return 0; }).Wait();
    }

    public event EventHandler<WebViewNavigationCompletedEventArgs>? NavigationCompleted;
    public event EventHandler<WebViewNavigationStartingEventArgs>? NavigationStarted;
    public event EventHandler? Initialized;

    public void Dispose()
    {
        RunOnGlibThread(() =>
        {
            _webView.Window?.Dispose();
            _webView.Dispose();
            return 0;
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

    public Task<string?> InvokeScript(string scriptName)
    {
        return RunOnGlibThread(() =>
        {
            _webView.RunJavascript(scriptName);
            return (string?)null;
        });
    }

    public void Navigate(Uri url)
    {
        RunOnGlibThread(() => { _webView.LoadUri(url.AbsolutePath); return 0; }).Wait();
    }

    public void NavigateToString(string text)
    {
        RunOnGlibThread(() =>
        {
            _webView.LoadHtml(text);
            return 0;
        }).Wait();
    }

    public bool Refresh()
    {
        return RunOnGlibThread(() => { _webView.Reload(); return true; }).GetAwaiter().GetResult();
    }

    public bool Stop()
    {
        return RunOnGlibThread(() => { _webView.StopLoading(); return true; }).GetAwaiter().GetResult();
    }
}
#endif
