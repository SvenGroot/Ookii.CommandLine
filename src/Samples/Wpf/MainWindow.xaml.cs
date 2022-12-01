using System.Windows;

namespace WpfSample;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly Arguments _arguments;

    public MainWindow(Arguments arguments)
    {
        _arguments = arguments;
        InitializeComponent();
    }

    public Arguments Arguments => _arguments;

    private void _helpButton_Click(object sender, RoutedEventArgs e)
    {
        var help = new UsageWindow();
        help.Show();
    }
}
