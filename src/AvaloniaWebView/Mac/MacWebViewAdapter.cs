#if MACOS
using System;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using WebKit;

namespace AvaloniaWebView.Mac;

internal sealed class MacWebViewAdapter : IWebViewAdapter
{
    private readonly WKWebView _webView;

    public MacWebViewAdapter()
    {
        _webView = new WKWebView(new CGRect(), new WKWebViewConfiguration
        {
        });
    }

    public IntPtr Handle => _webView.Handle.Handle;

    public string? HandleDescriptor => "NSView";
    
    public bool CanGoBack => _webView.CanGoBack;

    public bool CanGoForward => _webView.CanGoForward;

    public Uri? Source { get => _webView.Url; set => _webView.LoadRequest(new NSUrlRequest(value)); }

    public bool IsInitialized => true;
    public void SizeChanged() { }

    public event EventHandler<WebViewNavigationCompletedEventArgs>? NavigationCompleted;
    public event EventHandler<WebViewNavigationStartingEventArgs>? NavigationStarted;
    public event EventHandler? Initialized;

    public void Dispose() => _webView.Dispose();

    public bool GoBack() => _webView.GoBack() is not null;

    public bool GoForward() => _webView.GoForward() is not null;

    public async Task<string?> InvokeScript(string script)
    {
        var result = await _webView.EvaluateJavaScriptAsync(script);
        return result.ToString();
    }

    public void Navigate(Uri url) => _webView.LoadRequest(new NSUrlRequest(new NSUrl(url.AbsolutePath)));

    public void NavigateToString(string text) => _webView.LoadHtmlString(text, new NSUrl(NSBundle.MainBundle.BundlePath, true));

    public bool Refresh() => _webView.Reload() is not null;

    public bool Stop()
    {
        _webView.StopLoading();
        return true;
    }
}
#endif
