﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Input;
using Avalonia.Platform;

namespace AvaloniaWebView;

public class WebViewNavigationEventArgs : EventArgs
{
    public Uri? Request { get; init; }
}

public class WebViewNavigationCompletedEventArgs : WebViewNavigationEventArgs
{
    public bool IsSuccess { get; init; } = true;
}

public class WebViewNavigationWebPageRequestedEventArgs : WebViewNavigationEventArgs
{
    private readonly Func<CancellationToken, Task<Stream?>> _getStream;

    public WebViewNavigationWebPageRequestedEventArgs(Func<CancellationToken, Task<Stream?>> getStream)
    {
        _getStream = getStream;
    }

    public Task<Stream?> GetResponseStreamAsync(CancellationToken cancellationToken)
    {
        return _getStream(cancellationToken);
    }
}

public class WebViewNavigationStartingEventArgs : WebViewNavigationEventArgs
{
    public bool Cancel { get; set; }

    public bool? IsRedirected { get; init; }
}

internal interface IWebView
{
    /// <summary>
    /// NavigationCompleted dispatches after a navigate of the top level document completes rendering either successfully or not. 
    /// </summary>
    /// <remarks>
    /// On restricted platforms, including Browser, this event is fired only on programmatic navigation.
    /// </remarks>
    event EventHandler<WebViewNavigationCompletedEventArgs>? NavigationCompleted;

    /// <summary>
    /// NavigationStarting dispatches before a new navigate starts for the top level document.
    /// </summary>
    event EventHandler<WebViewNavigationStartingEventArgs>? NavigationStarted;

    /// <summary>
    /// WebPageRequested dispatches when an HTTP request has been made.
    /// </summary>
    event EventHandler<WebViewNavigationWebPageRequestedEventArgs>? WebPageRequested;

    /// <summary>
    /// Returns true if the webview can navigate to a previous page in the navigation history via the <see cref="GoBack"/> method.
    /// If the underlying native control is not yet initialized or navigation is not supported, this property is false.
    /// </summary>
    bool CanGoBack { get; }

    /// <summary>
    /// Returns true if the webview can navigate to a next page in the navigation history via the <see cref="GoForward"/> method.
    /// If the underlying native control is not yet initialized or navigation is not supported, this property is false.
    /// </summary>
    bool CanGoForward { get; }

    /// <summary>
    /// The Source property is the URI of the top level document of the WebView2. Setting the Source is equivalent to calling <see cref="Navigate"/>.
    /// </summary>
    Uri? Source { get; set; }

    /// <summary>
    /// Navigates to the previous page in navigation history.
    /// </summary>
    /// <returns>True if successfull. False if there is no page to navigate, native control is not yet initialized or navigation is not supported</returns>
    bool GoBack();

    /// <summary>
    /// Navigates to the next page in navigation history.
    /// </summary>
    /// <returns>True if successfull. False if there is no page to navigate, native control is not yet initialized or navigation is not supported</returns>
    bool GoForward();

    /// <summary>
    /// Executes the provided script in the top level document.
    /// </summary>
    Task<string?> InvokeScript(string scriptName);

    /// <summary>
    /// Causes a navigation of the top level document to the specified URI.
    /// </summary>
    void Navigate(Uri url);

    /// <summary>
    /// Renders the provided HTML as the top level document.
    /// </summary>
    void NavigateToString(string text);

    /// <summary>
    /// Reloads the top level document.
    /// </summary>
    /// <returns>True if successfull. False if not supported.</returns>
    bool Refresh();

    /// <summary>
    /// Stops any in progress navigation.
    /// </summary>
    /// <returns>True if successfull. False if not supported.</returns>
    bool Stop();
}

internal interface IWebViewAdapter : IWebView, IDisposable, IPlatformHandle
{
    event EventHandler? Initialized;

    bool IsInitialized { get; }

    void SizeChanged();
}
