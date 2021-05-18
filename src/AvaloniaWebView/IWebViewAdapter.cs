using System;

using Avalonia.Platform;

namespace AvaloniaWebBrowser
{
    internal interface IWebView
    {
        event EventHandler NavigationCompleted;

        bool CanGoBack { get; }

        bool CanGoForward { get; }

        Uri Source { get; set; }

        bool GoBack();

        bool GoForward();

        string? InvokeScript(string scriptName);

        void Navigate(Uri url);

        void NavigateToString(string text);

        void Refresh();

        void Stop();
    }

    internal interface IWebViewAdapter : IWebView
    {
        IPlatformHandle PlatformHandle { get; }
    }
}
