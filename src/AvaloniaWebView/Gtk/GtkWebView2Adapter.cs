#if !WINDOWS && !MACOS
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using GLib;
using Application = Gtk.Application;

namespace AvaloniaWebView.Gtk;

internal class GtkWebView2Adapter : IWebViewAdapter
{
    private readonly WebKit.WebView _webView;
    private readonly global::Gtk.Window _window;
    private static Task<IntPtr>? s_gtkTask;
    private readonly SynchronizationContext _gtkContext;

    [DllImport("libgdk-3.so.0")]
    public static extern IntPtr gdk_x11_window_get_xid(IntPtr window);

    public GtkWebView2Adapter()
    {
        var avContext = SynchronizationContext.Current;

        try
        {
            Application.Init();
            _gtkContext = SynchronizationContext.Current;

            var app = new Application($"avalonia.webview.{Guid.NewGuid():N}", ApplicationFlags.None);

            _window = new global::Gtk.Window(global::Gtk.WindowType.Toplevel);
            _window.Title = nameof(GtkWebView2Adapter);
            _window.KeepAbove = true;
            app.AddWindow(_window);

            _webView = new WebKit.WebView();
            _webView.Realize();
            _window.Add(_webView);
            _window.ShowAll();
            _window.Present();

            Handle = gdk_x11_window_get_xid(_window.Handle);
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(avContext);
        }
    }

    public IntPtr Handle { get; }
    public string HandleDescriptor => "XID";

    public bool IsInitialized => true;
    public void SizeChanged() {  }

    public bool CanGoBack => RunOnGlibThread(() => _webView.CanGoBack()).Result;

    public bool CanGoForward => RunOnGlibThread(() => _webView.CanGoForward()).Result;

    public Uri Source
    {
        get => RunOnGlibThread(() => new Uri(_webView.Uri)).Result;
        set => RunOnGlibThread(() => _webView.LoadUri(value?.AbsolutePath)).Wait();
    }

    public event EventHandler<WebViewNavigationCompletedEventArgs>? NavigationCompleted;
    public event EventHandler<WebViewNavigationStartingEventArgs>? NavigationStarted;
    public event EventHandler<WebViewNavigationWebPageRequestedEventArgs>? WebPageRequested;
    public event EventHandler? Initialized;

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
        RunOnGlibThread(() => _webView.LoadUri(url.AbsolutePath)).Wait();
    }

    public void NavigateToString(string text)
    {
        RunOnGlibThread(() =>
        {
            _webView.LoadHtml(text);
        }).Wait();
    }

    public bool Refresh()
    {
        RunOnGlibThread(() => _webView.Reload()).Wait();
        return true;
    }

    public bool Stop()
    {
        RunOnGlibThread(() => _webView.StopLoading()).Wait();
        return true;
    }
    
    private Task<T> RunOnGlibThread<T>(Func<T> action)
    {
        var tcs = new TaskCompletionSource<T>();
        _gtkContext.Post(static arg =>
        {
            var (tcs, action) = (ValueTuple<TaskCompletionSource<T>, Func<T>>)arg!;
            try
            {
                tcs.TrySetResult(action());
            }
            catch (OperationCanceledException)
            {
                tcs.TrySetCanceled();
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        }, (tcs, action));
        return tcs.Task;
    }
    
    private System.Threading.Tasks.Task RunOnGlibThread(Action action)
    {
        var tcs = new TaskCompletionSource();
        _gtkContext.Post(static arg =>
        {
            var (tcs, action) = (ValueTuple<TaskCompletionSource, Action>)arg!;
            try
            {
                action();
                tcs.TrySetResult();
            }
            catch (OperationCanceledException)
            {
                tcs.TrySetCanceled();
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        }, (tcs, action));
        return tcs.Task;
    }
}
#endif
