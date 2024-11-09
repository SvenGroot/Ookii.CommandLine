using Ookii.CommandLine;
using System.Net;

namespace WpfSample;

// This custom UsageWriter outputs usage as HTML.
internal class HtmlUsageWriter : UsageWriter
{
    // This is intended for use with GetUsage() only so no way to specify a writer.
    public HtmlUsageWriter()
        : base(null, TriState.False)
    {
        // The indents and blank lines don't alter how the output looks (whitespace is ignored in
        // HTML after all), but they're not needed either.
        SyntaxIndent = 0;
        ArgumentDescriptionIndent = 0;
        BlankLineAfterDescription = false;
    }

    // Wrap the entire usage in an HTML page.
    protected override void WriteParserUsageCore(UsageHelpRequest request)
    {
        Writer.WriteLine(HtmlHeader);
        base.WriteParserUsageCore(request);
        Writer.WriteLine(HtmlFooter);
    }

    // Header and paragraph for the description.
    protected override void WriteApplicationDescription(string description)
    {
        Writer.WriteLine("<h1>Description</h1>");
        Writer.Write("<p>");
        Write(description);
        Writer.Write("</p>");
    }

    // Header and paragraph for the syntax.
    protected override void WriteParserUsageSyntax()
    {
        Writer.WriteLine("<h1>Usage</h1>");
        Writer.Write("<p id=\"usage\"><code>");
        base.WriteParserUsageSyntax();
        Writer.Write("</code></p>");
    }

    // Omit "Usage:" (we use a header instead), and render the executable name in bold.
    protected override void WriteUsageSyntaxPrefix()
    {
        Writer.Write($"<strong>{ExecutableName}</strong>");
        // This application doesn't use subcommands, so we don't need to worry about the command
        // name.
    }

    // Prevent wrapping an argument name after the argument name prefix.
    protected override void WriteArgumentName(string argumentName, string prefix)
    {
        Writer.Write("<span class=\"argument\">");
        base.WriteArgumentName(argumentName, prefix);
        Writer.Write("</span>");
    }

    // Header and description list for descriptions.
    protected override void WriteArgumentDescriptions()
    {
        // The header is written here, and not in WriteArgumentDescriptionListHeader, because if we
        // did it there it would be inside the <dl>.
        Writer.Write("<h1>Arguments</h1>");
        Writer.Write("<dl>");
        base.WriteArgumentDescriptions();
        Writer.Write("</dl>");
    }

    // Wrap the argument name, value description, and aliases in a description term.
    protected override void WriteArgumentDescriptionHeader(CommandLineArgument argument)
    {
        Writer.Write("<dt>");
        base.WriteArgumentDescriptionHeader(argument);
        Writer.Write("</dt>");
    }

    // Wrap the description itself in a description definition.
    protected override void WriteArgumentDescriptionBody(CommandLineArgument argument)
    {
        Writer.Write("<dd>");
        base.WriteArgumentDescriptionBody(argument);
        Writer.Write("</dd>");
    }

    // Prevent wrapping after the argument name prefix.
    protected override void WriteArgumentNameForDescription(string argumentName, string prefix)
        => WriteArgumentName(argumentName, prefix);

    // Make sure all text is HTML encoded.
    protected override void Write(string? value)
    {
        WebUtility.HtmlEncode(value, Writer);
    }

    // Also HTML encode single character writes.
    protected override void Write(char value) => Write(value.ToString());

    private const string HtmlHeader = @"<!DOCTYPE html>
<html xmlns=""http://www.w3.org/1999/xhtml"" lang=""en"" xml:lang=""en"">
<head>
    <title>Usage help</title>
    <style type=""text/css"">
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            color: white;
            background-color: #1e1e1e;
            margin: 1em 2em;
        }
        h1 {
            font-size: 100%;
            margin-left: -1em;
            color: deepskyblue;
        }
        dt {
            color: lightgreen;
        }
        dd {
            margin-bottom: 1em;
        }
        #usage {
            text-indent: -2em;
            margin-left: 2em;
        }
        .argument {
            white-space: nowrap;
        }
    </style>
</head>
<body>";

    private const string HtmlFooter = "</body></html>";
}
