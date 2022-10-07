#if MACOS
using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Input;
using Avalonia.Platform;

using MonoMac.AppKit;
using MonoMac.Foundation;

namespace AvaloniaWebView.Mac;

internal sealed class MacWebViewAdapter : IWebViewAdapter, IDisposable
{
    private readonly MonoMac.WebKit.WebView _webView;

    public MacWebViewAdapter()
    {
        _webView = new MonoMac.WebKit.WebView();

        _webView.FinishedLoad += (s, a) => NavigationCompleted?
            .Invoke(this, new WebViewNavigationCompletedEventArgs() { Request = new Uri(_webView.MainFrameUrl) });
    }

    public bool CanGoBack => _webView.CanGoBack();

    public bool CanGoForward => _webView.CanGoForward();

    public Uri Source { get => new(_webView.MainFrameUrl); set => _webView.MainFrameUrl = value.OriginalString; }

    public bool IsInitialized => true;
    public IPlatformHandle PlatformHandle => new MacViewHandle(_webView);

    public event EventHandler<WebViewNavigationCompletedEventArgs>? NavigationCompleted;
    public event EventHandler<WebViewNavigationStartingEventArgs>? NavigationStarted;
    public event EventHandler<WebViewNavigationWebPageRequestedEventArgs>? WebPageRequested;
    public event EventHandler? Initialized;

    public void Dispose() => _webView.Dispose();

    public bool GoBack() => _webView.GoBack();

    public bool GoForward() => _webView.GoForward();

    public void HandleResize(PixelSize size)
    {
    }

    public Task<string> InvokeScript(string script) => Task.FromResult(_webView.StringByEvaluatingJavaScriptFromString(script));

    public void Navigate(Uri url) => _webView.MainFrame.LoadRequest(new NSUrlRequest(new NSUrl(url.AbsolutePath)));

    public void NavigateToString(string text) => _webView.MainFrame.LoadHtmlString(text, null);

    public bool Refresh()
    {
        _webView.MainFrame.Reload();
        return true;
    }

    public bool Stop()
    {
        _webView.MainFrame.StopLoading();
        return true;
    }
}

internal class MacViewHandle : IPlatformHandle, IDisposable
{
    private NSView? _view;

    public MacViewHandle(NSView view)
    {
        _view = view;
    }

    public IntPtr Handle => _view?.Handle ?? IntPtr.Zero;
    public string HandleDescriptor => "NSView";

    public void Dispose()
    {
        _view?.Dispose();
        _view = null;
    }
}
#endif
