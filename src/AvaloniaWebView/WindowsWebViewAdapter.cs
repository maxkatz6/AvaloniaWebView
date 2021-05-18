#if WINDOWS
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Windows.Forms;

using Avalonia.Platform;

using Microsoft.Win32;

using WFWebBrowser = System.Windows.Forms.WebBrowser;

namespace AvaloniaWebBrowser
{
    [SupportedOSPlatform("Windows")]
    internal sealed class WindowsWebViewAdapter : IWebViewAdapter, IDisposable
    {
        private readonly WFWebBrowser _webBrowser;

        static WindowsWebViewAdapter()
        {
            using (var key = Registry.CurrentUser
                .OpenSubKey(@"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", true))
            {
                var app = Path.GetFileName(Application.ExecutablePath);
                key?.SetValue(app, 11001, RegistryValueKind.DWord);
                key?.Close();
            }
        }

        public WindowsWebViewAdapter()
        {
            _webBrowser = new WFWebBrowser
            {
                ScriptErrorsSuppressed = true,
                IsWebBrowserContextMenuEnabled = false
            };

            _webBrowser.Navigated += (s, a) => NavigationCompleted?.Invoke(this, EventArgs.Empty);
        }

        public bool CanGoBack => _webBrowser.CanGoBack;

        public bool CanGoForward => _webBrowser.CanGoForward;

        public Uri Source { get => _webBrowser.Url; set => _webBrowser.Url = value; }

        public IPlatformHandle PlatformHandle => new PlatformHandle(_webBrowser.Handle, "HWND");

        public event EventHandler? NavigationCompleted;

        public void Dispose()
        {
            WinAPI.DestroyWindow(_webBrowser.Handle);
            _webBrowser.Dispose();
        }

        public bool GoBack() => _webBrowser.GoBack();

        public bool GoForward() => _webBrowser.GoForward();

        public string? InvokeScript(string script) => _webBrowser.Document.InvokeScript("eval", new object[] { script }) as string;

        public void Navigate(Uri url) => _webBrowser.Navigate(url);

        public void NavigateToString(string text)
        {
            _webBrowser.DocumentText = "0";
            _webBrowser.Document.OpenNew(true);
            _webBrowser.Document.Write(text);
            _webBrowser.Refresh();
        }

        public void Refresh() => _webBrowser.Refresh();

        public void Stop() => _webBrowser.Stop();

        public void EnsureSize(int width, int height)
        {
            _webBrowser.Width = width;
            _webBrowser.Height = height;
        }
    }

    internal static class WinAPI
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool DestroyWindow(IntPtr hwnd);
    }
}
#endif
