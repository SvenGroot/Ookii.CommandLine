using Ookii.CommandLine.Conversion;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace SubcommandSample;

// A ArgumentConverter for the Encoding class, using the utility base class provided by
// Ookii.CommandLine.
internal class EncodingConverter : ArgumentConverter
{
    public override object? Convert(string value, CultureInfo culture)
    {
        try
        {
            return Encoding.GetEncoding(value);
        }
        catch (ArgumentException ex)
        {
            // This is the expected exception type for a converter.
            throw new FormatException(ex.Message, ex);
        }
    }
}
