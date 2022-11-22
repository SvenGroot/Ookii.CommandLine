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
