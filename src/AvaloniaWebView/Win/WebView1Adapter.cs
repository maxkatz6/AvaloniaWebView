#if WINDOWS
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

using Windows.Web.UI;
using Windows.Web.UI.Interop;
using Avalonia.Platform;

namespace AvaloniaWebView.Win;

[SupportedOSPlatform("Windows10.0.17134")]
internal sealed class WebView1Adapter : IWebViewAdapter
{
    private static WebViewControlProcess? s_lazyProcess;
    private static int s_webViewsCount;
    
    private WebViewControl? _webViewControl;
    private Action? _subscriptions;

    public WebView1Adapter(IPlatformHandle handle)
    {
        Handle = handle.Handle;
        Initialize();

        Interlocked.Increment(ref s_webViewsCount);
    }
    
    public IntPtr Handle { get; }
    public string? HandleDescriptor => "HWDN";

    private async void Initialize()
    {
        var process = s_lazyProcess ??= CreateProcess();

        var control = await process.CreateWebViewControlAsync(Handle,
            new Windows.Foundation.Rect(0, 0, 100, 100));

        _webViewControl = control;
        _webViewControl.IsVisible = true;
        SizeChanged();

        _subscriptions = AddHandlers(_webViewControl);

        IsInitialized = true;
        Initialized?.Invoke(this, EventArgs.Empty);
    }
    
    public bool IsInitialized { get; private set; }

    public bool CanGoBack => _webViewControl?.CanGoBack ?? false;

    public bool CanGoForward => _webViewControl?.CanGoForward ?? false;

    public Uri Source
    {
        [return: MaybeNull] get
        {
            return _webViewControl?.Source!;
        }
        set
        {
            Navigate(value);
        }
    }

    public event EventHandler<WebViewNavigationCompletedEventArgs>? NavigationCompleted;
    public event EventHandler<WebViewNavigationStartingEventArgs>? NavigationStarted;
    public event EventHandler<WebViewNavigationWebPageRequestedEventArgs>? WebPageRequested;
    public event EventHandler? Initialized;

    public bool GoBack()
    {
        if (!CanGoBack) return false;
        _webViewControl?.GoBack();
        return true;
    }

    public bool GoForward()
    {
        if (!CanGoForward) return false;
        _webViewControl?.GoForward();
        return true;
    }

    public Task<string?> InvokeScript(string scriptName)
    {
        return _webViewControl?.InvokeScriptAsync(scriptName, null).AsTask() ?? Task.FromResult<string?>(null);
    }

    public void Navigate(Uri url)
    {
        if (_webViewControl is not null)
        {
            _webViewControl.Source = url;
        }
    }

    public void NavigateToString(string text)
    {
        _webViewControl?.NavigateToString(text);
    }

    public bool Refresh()
    {
        _webViewControl?.Refresh();
        return true;
    }

    public bool Stop()
    {
        _webViewControl?.Stop();
        return true;
    }

    public void SizeChanged()
    {
        WinApiHelpers.GetWindowRect(Handle, out var rect);

        if (_webViewControl is not null)
        {
            _webViewControl.Bounds = new Windows.Foundation.Rect(0, 0, rect.Right - rect.Left, rect.Bottom - rect.Top);
        }
    }
    
    private static WebViewControlProcess CreateProcess()
    {
        var options = new WebViewControlProcessOptions();
        options.PrivateNetworkClientServerCapability = WebViewControlProcessCapabilityState.Enabled;
        return new WebViewControlProcess(options);
    }

    private Action AddHandlers(WebViewControl webView)
    {
        webView.NavigationStarting += WebViewOnNavigationStarting;
        void WebViewOnNavigationStarting(object? sender, WebViewControlNavigationStartingEventArgs e)
        {
            var args = new WebViewNavigationStartingEventArgs { Request = e.Uri };
            NavigationStarted?.Invoke(this, args);
            if (args.Cancel)
            {
                e.Cancel = true;
            }
        }

        webView.NavigationCompleted += WebViewOnNavigationCompleted;
        void WebViewOnNavigationCompleted(object? sender, WebViewControlNavigationCompletedEventArgs e)
        {
            NavigationCompleted?.Invoke(this, new WebViewNavigationCompletedEventArgs
            {
                Request = ((WebViewControl)sender!).Source,
                IsSuccess = e.IsSuccess
            });
        }
        
        webView.WebResourceRequested += WebViewOnWebResourceRequested;
        void WebViewOnWebResourceRequested(object? sender, WebViewControlWebResourceRequestedEventArgs e)
        {
            WebPageRequested?.Invoke(this, new WebViewNavigationWebPageRequestedEventArgs(async ct =>
            {
                if (e.Request.Content is not null)
                {
                    var stream = await e.Request.Content.ReadAsInputStreamAsync().AsTask(ct);
                    return stream.AsStreamForRead();
                }
                return null;
            })
            {
                Request = e.Request.RequestUri
            });
        }

        return () =>
        {
            webView.NavigationStarting -= WebViewOnNavigationStarting;
            webView.NavigationCompleted -= WebViewOnNavigationCompleted;
            webView.WebResourceRequested -= WebViewOnWebResourceRequested;
        };
    }

    public void Dispose()
    {
        var webViewsCount = Interlocked.Decrement(ref s_webViewsCount);

        _subscriptions?.Invoke();

        _webViewControl?.Close();
        _webViewControl = null;

        if (s_lazyProcess is { } process && webViewsCount == 0)
        {
            s_lazyProcess = null;
        }
    }
}
#endif
