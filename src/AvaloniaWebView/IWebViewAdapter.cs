using System;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Platform;

namespace AvaloniaWebView;

public class WebViewNavigationEventArgs : EventArgs
{
    public Uri? Request { get; init; }
}

internal interface IWebView
{
    event EventHandler<WebViewNavigationEventArgs>? NavigationCompleted;

    event EventHandler<WebViewNavigationEventArgs>? NavigationStarted;

    bool CanGoBack { get; }

    bool CanGoForward { get; }

    Uri? Source { get; set; }

    bool GoBack();

    bool GoForward();

    string? InvokeScript(string scriptName);

    void Navigate(Uri url);

    Task NavigateToString(string text);

    void Refresh();

    void Stop();
}

internal interface IWebViewAdapter : IWebView
{
    IPlatformHandle PlatformHandle { get; }

    void HandleResize(int width, int height, float zoom);

    bool HandleKeyDown(Key key, KeyModifiers keyModifiers);
}
