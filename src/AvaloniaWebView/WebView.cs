using System;
using System.Runtime.InteropServices;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;

namespace AvaloniaWebBrowser
{
    public class WebView : NativeControlHost, IWebView
    {
        private IWebViewAdapter? _webViewAdapter;

        public bool CanGoBack => _webViewAdapter?.CanGoBack ?? false;

        public bool CanGoForward => _webViewAdapter?.CanGoForward ?? false;

        public Uri Source
        {
            get => _webViewAdapter?.Source ?? throw new InvalidOperationException("Control was not initialized");
            set
            {
                if (_webViewAdapter is null)
                {
                    throw new InvalidOperationException("Control was not initialized");
                }
                _webViewAdapter.Source = value;
            }
        }

        public event EventHandler? NavigationCompleted;

        public bool GoBack() => _webViewAdapter?.GoBack() ?? throw new InvalidOperationException("Control was not initialized");

        public bool GoForward() => _webViewAdapter?.GoForward() ?? throw new InvalidOperationException("Control was not initialized");

        public string? InvokeScript(string scriptName) => _webViewAdapter is null
            ? throw new InvalidOperationException("Control was not initialized")
            : _webViewAdapter.InvokeScript(scriptName);

        public void Navigate(Uri url) => (_webViewAdapter ?? throw new InvalidOperationException("Control was not initialized"))
            .Navigate(url);

        public void NavigateToString(string text) => (_webViewAdapter ?? throw new InvalidOperationException("Control was not initialized"))
            .NavigateToString(text);

        public void Refresh() => (_webViewAdapter ?? throw new InvalidOperationException("Control was not initialized"))
            .Refresh();

        public void Stop() => (_webViewAdapter ?? throw new InvalidOperationException("Control was not initialized"))
            .Stop();

        protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
        {
#if WINDOWS
            _webViewAdapter = new WindowsWebViewAdapter();
            SubscribeOnEvents();
            return _webViewAdapter.PlatformHandle;
#else
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                throw new NotSupportedException("Linux OS is not supported.");
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new NotSupportedException("Only NET5.0-WINDOWS Nuget package is supported on Windows OS.");
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                _webViewAdapter = new MacWebViewAdapter();
                SubscribeOnEvents();
                return _webViewAdapter.PlatformHandle;
            }
            return base.CreateNativeControlCore(parent);
#endif
        }

        private void SubscribeOnEvents()
        {
            if (_webViewAdapter is not null)
            {
                _webViewAdapter.NavigationCompleted += WebViewAdapter_NavigationCompleted;
            }
        }

        private void WebViewAdapter_NavigationCompleted(object? sender, EventArgs e)
        {
            NavigationCompleted?.Invoke(this, e);
        }

#if WINDOWS
        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == BoundsProperty
                && _webViewAdapter is WindowsWebViewAdapter windowsWebViewAdapter)
            {
                var newValue = change.NewValue.GetValueOrDefault<Rect>();
                var scaling = VisualRoot.RenderScaling;
                windowsWebViewAdapter.EnsureSize((int)(newValue.Width * scaling), (int)(newValue.Height * scaling));
            }
        }
#endif

        protected override void DestroyNativeControlCore(IPlatformHandle control)
        {
            if (_webViewAdapter is not null)
            {
                _webViewAdapter.NavigationCompleted -= WebViewAdapter_NavigationCompleted;
                (_webViewAdapter as IDisposable)?.Dispose();
            }
        }
    }
}
