using System;

namespace AvaloniaWebView;

public class WebViewCapabilities
{
    private static bool? _isMsWebView2Available;
    public static bool IsMsWebView2Available => _isMsWebView2Available ??= IsMsWebView2AvailableInternal();

    public static bool IsMsWebView1Available => OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17134);

    private static bool IsMsWebView2AvailableInternal()
    {
#if WINDOWS
        try
        {
            var versionString = Microsoft.Web.WebView2.Core.CoreWebView2Environment.GetAvailableBrowserVersionString();
            return !string.IsNullOrWhiteSpace(versionString);
        }
        catch (System.Exception)
        {
            return false;
        }
#else
        return false;
#endif
    }
}
