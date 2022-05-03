using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform;

namespace AvaloniaWebView;


public class NativeWebView : NativeControlHost, IWebView
{
    private IWebViewAdapter? _webViewAdapter;
    private Uri? _delayedSource;
    private TaskCompletionSource _webViewReadyCompletion = new();

    public event EventHandler<WebViewNavigationEventArgs>? NavigationCompleted;

    public event EventHandler<WebViewNavigationEventArgs>? NavigationStarted;

    public bool CanGoBack => _webViewAdapter?.CanGoBack ?? false;

    public bool CanGoForward => _webViewAdapter?.CanGoForward ?? false;

    public Uri? Source
    {
        get => _webViewAdapter?.Source ?? throw new InvalidOperationException("Control was not initialized");
        set
        {
            if (_webViewAdapter is null)
            {
                _delayedSource = value;
                return;
            }
            _webViewAdapter.Source = value;
        }
    }

    public bool GoBack()
    {
        return _webViewAdapter?.GoBack() ?? throw new InvalidOperationException("Control was not initialized");
    }

    public bool GoForward()
    {
        return _webViewAdapter?.GoForward() ?? throw new InvalidOperationException("Control was not initialized");
    }

    public string? InvokeScript(string scriptName)
    {
        return _webViewAdapter is null
            ? throw new InvalidOperationException("Control was not initialized")
            : _webViewAdapter.InvokeScript(scriptName);
    }

    public void Navigate(Uri url)
    {
        (_webViewAdapter ?? throw new InvalidOperationException("Control was not initialized"))
            .Navigate(url);
    }

    public Task NavigateToString(string text)
    {
        return (_webViewAdapter ?? throw new InvalidOperationException("Control was not initialized"))
            .NavigateToString(text);
    }

    public void Refresh()
    {
        (_webViewAdapter ?? throw new InvalidOperationException("Control was not initialized"))
            .Refresh();
    }

    public void Stop()
    {
        (_webViewAdapter ?? throw new InvalidOperationException("Control was not initialized"))
            .Stop();
    }

    public Task WaitForNativeHost()
    {
        return _webViewReadyCompletion.Task;
    }

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        IPlatformHandle handle;
#if WINDOWS
        _webViewAdapter = WebViewCapabilities.IsWebView2Available
            ? new WindowsWebView2Adapter()
            : new WindowsWebBrowserAdapter();
        SubscribeOnEvents();
        handle = _webViewAdapter.PlatformHandle;
#else
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new NotSupportedException("Only NET6.0-WINDOWS Nuget package is supported on Windows OS.");
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                _webViewAdapter = new GtkWebView2Adapter();
                SubscribeOnEvents();
                handle = _webViewAdapter.PlatformHandle;
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                _webViewAdapter = new MacWebViewAdapter();
                SubscribeOnEvents();
                handle = _webViewAdapter.PlatformHandle;
            }
            else
            {
                return base.CreateNativeControlCore(parent);
            }
#endif

        if (_delayedSource is not null)
        {
            _webViewAdapter.Source = _delayedSource;
        }

        _webViewReadyCompletion.TrySetResult();

        return handle;
    }

    private void SubscribeOnEvents()
    {
        if (_webViewAdapter is not null)
        {
            _webViewAdapter.NavigationStarted += WebViewAdapterOnNavigationStarted;
            _webViewAdapter.NavigationCompleted += WebViewAdapterOnNavigationCompleted;
        }
    }

    private void WebViewAdapterOnNavigationStarted(object? sender, WebViewNavigationEventArgs e)
    {
        NavigationStarted?.Invoke(this, e);
    }

    private void WebViewAdapterOnNavigationCompleted(object? sender, WebViewNavigationEventArgs e)
    {
        NavigationCompleted?.Invoke(this, e);
    }

    protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == BoundsProperty)
        {
            var newValue = change.NewValue.GetValueOrDefault<Rect>();
            var scaling = (float)(VisualRoot?.RenderScaling ?? 1.0f);
            _webViewAdapter?.HandleResize((int)(newValue.Width * scaling), (int)(newValue.Height * scaling), scaling);
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (_webViewAdapter != null)
        {
            e.Handled = _webViewAdapter.HandleKeyDown(e.Key, e.KeyModifiers);
        }

        base.OnKeyDown(e);
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        if (_webViewAdapter is not null)
        {
            _webViewReadyCompletion = new TaskCompletionSource();
            _webViewAdapter.NavigationStarted -= WebViewAdapterOnNavigationStarted;
            _webViewAdapter.NavigationCompleted -= WebViewAdapterOnNavigationCompleted;
            (_webViewAdapter as IDisposable)?.Dispose();
        }
    }
}
