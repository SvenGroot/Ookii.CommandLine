using Ookii.CommandLine;
using System.Windows;

namespace WpfSample;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class UsageWindow : Window
{
    private readonly CommandLineParser<Arguments> _parser;

    public UsageWindow(CommandLineParser<Arguments>? parser = null)
    {
        InitializeComponent();
        _parser = parser ?? new();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await _webView.EnsureCoreWebView2Async();
        _webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;

        // Generate and display usage.
        string usage = _parser.GetUsage(new HtmlUsageWriter());
        _webView.NavigateToString(usage);
    }
}
