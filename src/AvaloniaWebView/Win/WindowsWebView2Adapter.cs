#if WINDOWS
using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Platform;

using Microsoft.Web.WebView2.WinForms;

namespace AvaloniaWebView;

[SupportedOSPlatform("Windows")]
internal class WindowsWebView2Adapter : IWebViewAdapter, IDisposable
{
    private readonly WebView2 _webView;

    public WindowsWebView2Adapter()
    {
        _webView = new WebView2();

        _webView.NavigationStarting += (s, a) =>
        {
            NavigationStarted?.Invoke(this, new WebViewNavigationEventArgs { Request = new Uri(a.Uri) });
        };
        _webView.NavigationCompleted += (s, a) =>
        {
            NavigationCompleted?.Invoke(this, new WebViewNavigationEventArgs { Request = _webView.Source });
        };
    }

    public IPlatformHandle PlatformHandle => new PlatformHandle(_webView.Handle, "HWND");

    public bool CanGoBack => _webView.CanGoBack;

    public bool CanGoForward => _webView.CanGoForward;

    public Uri? Source
    {
        get => _webView.Source;
        set => _webView.Source = value;
    }

    public event EventHandler<WebViewNavigationEventArgs>? NavigationCompleted;
    public event EventHandler<WebViewNavigationEventArgs>? NavigationStarted;

    public void Dispose()
    {
        _webView.Dispose();
    }

    public bool GoBack()
    {
        _webView.GoBack();
        return true;
    }

    public bool GoForward()
    {
        _webView.GoForward();
        return true;
    }

    public string? InvokeScript(string scriptName)
    {
        return _webView.ExecuteScriptAsync(scriptName).GetAwaiter().GetResult();
    }

    public void Navigate(Uri url)
    {
        _webView.Source = url;
    }

    public async Task NavigateToString(string text)
    {
        await _webView.EnsureCoreWebView2Async();

        _webView.NavigateToString(text);
    }

    public void Refresh()
    {
        _webView.Refresh();
    }

    public void Stop()
    {
        _webView.Stop();
    }

    public void HandleResize(int width, int height, float zoom)
    {
    }

    public bool HandleKeyDown(Key key, KeyModifiers keyModifiers)
    {
        return false;
    }
}
#endif
