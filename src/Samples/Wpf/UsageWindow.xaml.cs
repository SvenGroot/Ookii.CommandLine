using Ookii.CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace wpftest3;

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
