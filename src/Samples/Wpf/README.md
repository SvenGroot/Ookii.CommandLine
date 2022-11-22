# WPF sample

This sample demonstrates how you can use Ookii.CommandLine in an application with a graphical user
interface. It uses the same arguments as the [Parser sample](../Parser).

Running this sample requires Microsoft Windows.

This sample does not use the static `CommandLineParser.Parse<T>()` method, but instead handles
errors manually so it can show a dialog with the error message and a help button, and show the
usage help only if that button was clicked, or the "-Help" argument was used.

To use as much of the built-in usage help generation as possible, this sample uses a custom
UsageWriter that wraps the various components of the help in an HTML page, and then displays that
to the user using a [WebView2 control](https://learn.microsoft.com/en-us/microsoft-edge/webview2/).

![WPF usage help in a WebView2 control](../../../docs/images/wpf.png)

The sample uses a simple CSS stylesheet to format the usage help; you can make this as fancy as you
like, of course.

This is by no means the only way. Since all the information needed to generate usage help is
available in the `CommandLineParser` class, you could just as easily forego the `UsageWriter`
entirely and use XAML page to show the usage.

This sample also defines a custom "-Version" argument. The automatic one that gets added by
Ookii.CommandLine writes to the console, so isn't useful here. This manual implementation shows
the version information in a dialog box.

A similar approach would work for Windows Forms, or other GUI framework.

This application is very basic; it's just a sample, and I don't do a lot of GUI work nowadays. Sorry.
