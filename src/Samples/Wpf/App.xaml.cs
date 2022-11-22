using Ookii.CommandLine;
using Ookii.Dialogs.Wpf;
using System.Windows;

namespace wpftest3
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var parser = new CommandLineParser<Arguments>();
            try
            {
                var args = parser.Parse(e.Args);
                if (args != null)
                {
                    var window = new MainWindow(args);
                    window.Show();
                    return;
                }
            }
            catch (CommandLineArgumentException ex)
            {
                // Use a TaskDialog (from Ookii.Dialogs) so we can have a help button.
                var dialog = new TaskDialog()
                {
                    MainInstruction = "Invalid command line",
                    Content = ex.Message,
                    WindowTitle = parser.ApplicationFriendlyName,
                    MainIcon = TaskDialogIcon.Error,
                    AllowDialogCancellation = true,
                };

                dialog.Buttons.Add(new TaskDialogButton(ButtonType.Close) { Default = true });
                var helpButton = new TaskDialogButton("&Help");
                dialog.Buttons.Add(helpButton);
                var button = dialog.Show();

                // Don't show help unless the help button was used.
                if (button != helpButton)
                {
                    parser.HelpRequested = false;
                }
            }

            if (parser.HelpRequested)
            {
                var help = new UsageWindow(parser);
                help.Show();
                return;
            }

            // Need to shutdown manually if we didn't show any window.
            Shutdown();
        }
    }
}
