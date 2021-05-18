﻿#if !WINDOWS
using System;

using Avalonia.Platform;

using MonoMac.AppKit;
using MonoMac.Foundation;

namespace AvaloniaWebBrowser
{
    internal sealed class MacWebViewAdapter : IWebViewAdapter, IDisposable
    {
        private readonly MonoMac.WebKit.WebView _webView;

        public MacWebViewAdapter()
        {
            _webView = new MonoMac.WebKit.WebView();

            _webView.FinishedLoad += (s, a) => NavigationCompleted?.Invoke(this, EventArgs.Empty);
        }

        public bool CanGoBack => _webView.CanGoBack();

        public bool CanGoForward => _webView.CanGoForward();

        public Uri Source { get => new(_webView.MainFrameUrl); set => _webView.MainFrameUrl = value.OriginalString; }

        public IPlatformHandle PlatformHandle => new MacViewHandle(_webView);

        public event EventHandler? NavigationCompleted;

        public void Dispose() => _webView.Dispose();

        public bool GoBack() => _webView.GoBack();

        public bool GoForward() => _webView.GoForward();

        public string InvokeScript(string script) => _webView.StringByEvaluatingJavaScriptFromString(script);

        public void Navigate(Uri url) => _webView.MainFrame.LoadRequest(new NSUrlRequest(new NSUrl(url.AbsolutePath)));

        public void NavigateToString(string text) => _webView.MainFrame.LoadHtmlString(text, null);

        public void Refresh() => _webView.MainFrame.Reload();

        public void Stop() => _webView.MainFrame.StopLoading();
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
}
#endif
