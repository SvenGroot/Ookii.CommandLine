﻿using Ookii.CommandLine;
using Ookii.Dialogs.Wpf;
using System.Windows;

namespace WpfSample;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // Use the CommandLineParser<T> class, instead of the static Parse() method, so we can
        // manually handle errors. The GeneratedParserAttribute generates a CreateParser() method
        // for this purpose.
        var parser = Arguments.CreateParser();
        bool showHelp = true;
        try
        {
            var args = parser.Parse(e.Args);
            if (args != null)
            {
                // Arguments were parsed successfully.
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
                showHelp = false;
            }
        }

        if (parser.ParseResult.HelpRequested && showHelp)
        {
            var help = new UsageWindow(parser);
            help.Show();
            return;
        }

        // Need to shutdown manually if we didn't show any window.
        Shutdown();
    }
}
