namespace AvaloniaWebView;

public class WebViewCapabilities
{
    private static bool? _isWebView2Available;
    public static bool IsWebView2Available => _isWebView2Available ??= IsWebView2AvailableInternal();

    private static bool IsWebView2AvailableInternal()
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
