# AvaloniaWebView

NativeWebView control for Avalonia that attempts to use native web view implementations without heavy dependencies on Chromium or any other non-native browser implementation which have to be shipped with the app.
Please, don't use this project in production, since it requires more work and stabilization to be a universal tool for every project. But rather use it as a reference for your own minimal web view implementation specifically for your needs.

If you want an easier and more reliable solution for embedding web view pages into your app - look for CEF wrappers in Avalonia. They use the Chromium framework and are easier to use.

### Status of backends

## Windows

WebView2 is used when available.
Otherwise fallbacks to the Internet Explorer WebBrowser component.
Either way, it requires developers to target "net6.0-window" framework, which is buildable only on Windows.

In both cases, WinForms wrappers are used.
Should be possible to generate more lightweight adapters using MicroCOM or CsWinRT/CsWin32. Not only bundle size will benefit from it, but also remove dependency on "net6.0-window" target.
It's also possible to use WebView1 (based on old Edge).

## MacOS

Old WebView is used with MonoMAC.NetStandard, a glue layer between Obj-C and .NET. Relatavely easy to write your own mappings for new WkWebView.
A new target framework for "net6.0-macos" might be added to avoid MonoMAC dependency and use modern .NET MacOS APIs.

## Linux X11 GTK

GtkSharp is used, but Avalonia doesn't have proper XEmbed support for navigation and keyboard. Furthermore current implementation in this repository doesn't work at all.

## Linux Wayland

Control embedding isn't possible there.

## iOS/Android/WASM

Avalonia currently doesn't support native embed controls for these backends. But it might be possible in the future.
